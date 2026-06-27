# File Storage Architecture (MinIO via API Gateway)

This document describes how to handle file uploads and downloads through your **API** **without exposing MinIO publicly**. Use this pattern across any project that needs object storage behind a backend.

## Goals

- Keep MinIO on a **private network** (Docker internal network, VPC, etc.)
- Never expose MinIO credentials to clients (browser, mobile, third-party apps)
- Keep the storage bucket **private** (no public bucket policy)
- Use the **API as the only public entry point** for file operations
- Support optional **time-limited read URLs** when direct streaming through the API is not ideal

## High-Level Architecture

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Client    │ ──────► │     API     │ ──────► │    MinIO    │
│ (Web/Mobile)│ ◄────── │  (public)   │ ◄────── │  (private)  │
└─────────────┘         └─────────────┘         └─────────────┘
                              │
                              ▼
                        ┌─────────────┐
                        │   Database  │
                        │ (file paths)│
                        └─────────────┘
```

**Key principle:** Clients talk to the API. Only the API talks to MinIO.

## What We Do NOT Do

| Approach | Used here? | Why not |
|----------|------------|---------|
| Public MinIO bucket | No | Anyone could read/write objects |
| Public MinIO endpoint for uploads | No | Exposes storage layer and credentials risk |
| Pre-signed PUT URLs for client upload | No | Not needed; API proxies uploads instead |
| Storing MinIO credentials in frontend | No | Security violation |

## Upload Flow (Recommended Pattern)

Uploads use a **two-phase pattern**: stage first, then attach to a business entity.

### Phase 1 — Stage file in temporary folder

```http
POST /api/v1/files/uploads
Content-Type: multipart/form-data

file: <binary>
```

**What happens server-side:**

1. API receives the file
2. API generates a server-side object key: `{Guid}{extension}`
3. API uploads to MinIO via SDK (`PutObjectAsync`) into a staging prefix, e.g. `tmp/`
4. API returns the **object path only** (not a MinIO URL)

**Example response:**

```json
{
  "filePath": "tmp/3f2a1b9c-7e4d-4a1a-9c2b-1a2b3c4d5e6f.pdf",
  "contentType": "application/pdf",
  "fileName": "invoice.pdf"
}
```

### Phase 2 — Attach file to a domain entity

When creating a record (attachment, payment proof, profile image, etc.), the client sends the staged path:

```http
POST /api/v1/{entity}/{id}/attachments
Content-Type: application/json

{
  "path": "tmp/3f2a1b9c-7e4d-4a1a-9c2b-1a2b3c4d5e6f.pdf",
  "fileName": "invoice.pdf",
  "contentType": "application/pdf"
}
```

**What happens server-side:**

1. API verifies the object exists in MinIO
2. API moves the file from `tmp/` to a permanent prefix (e.g. `attachments/`, `profiles/`, `documents/`)
3. API stores the permanent path in the database
4. Temporary files can be cleaned up by a scheduled job

### Why two phases?

- Prevents orphan files when a user abandons a form
- Allows validation before the file is linked to business data
- Keeps object keys server-controlled (`Guid`-based names)
- Enables folder-based isolation per domain (`attachments/`, `profiles/`, etc.)

## Download Flow (Choose One)

### Option A — API proxy download (preferred for protected files)

```http
GET /api/v1/files/download?objectName=attachments/3f2a1b9c-....pdf
```

- API fetches from MinIO and streams the file to the client
- MinIO host is never exposed to the client
- Protect with authentication and permission checks in production

### Option B — Pre-signed GET URL (optional, read-only)

```http
GET /api/v1/files/url?objectName=attachments/3f2a1b9c-....pdf
```

**Example response:**

```json
{
  "url": "http://minio-internal:9000/app-bucket/attachments/...?X-Amz-..."
}
```

- API generates a **time-limited GET URL** using `PresignedGetObjectAsync`
- Bucket remains private; only this specific object is readable until expiry
- URL may reveal the MinIO host — keep MinIO on a private network and use short TTL (5–15 minutes recommended)
- **Only used for reads.** Do not use pre-signed PUT for uploads.

### Option C — Anonymous API proxy (for intentionally public assets)

```http
GET /api/v1/files/get-file?fileLocation=profiles/abc.jpg
```

- API streams the file without requiring the client to hit MinIO
- Public URL is the **API URL**, not MinIO
- Use only for assets that are safe to expose (e.g. profile images, public documents)

## Server-Side Components

### MinIO client registration (credentials stay on server)

```csharp
services.AddSingleton<IMinioClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;

    return new MinioClient()
        .WithEndpoint(settings.Endpoint)      // e.g. minio:9000 (internal)
        .WithCredentials(settings.AccessKey, settings.SecretKey)
        .WithSSL(settings.WithSSL)
        .Build();
});

services.AddSingleton<IStorageService, MinioStorageService>();
```

### Configuration (`MinioSettings`)

```json
{
  "MinioSettings": {
    "Endpoint": "minio:9000",
    "AccessKey": "<from-secrets>",
    "SecretKey": "<from-secrets>",
    "BucketName": "app-bucket",
    "WithSSL": false
  }
}
```

In Docker Compose, MinIO runs as an internal service. The API container connects via the service name (`minio:9000`). Clients never need the MinIO hostname.

### Core service operations

| Operation | Method | Purpose |
|-----------|--------|---------|
| Upload | `UploadFileAsync` | Stream file to MinIO |
| Download | `DownloadFileAsync` | Stream file from MinIO through API |
| Pre-signed URL | `GenerateFileUrlAsync` | Time-limited read URL (GET only) |
| Exists | `FileExistsAsync` | Validate staged file before attach |
| Move | `MoveFileToFolderAsync` | Promote `tmp/` → permanent prefix |
| Delete | `DeleteFileAsync` | Remove object |

## Folder Structure Convention

| Prefix | Purpose |
|--------|---------|
| `tmp/` | Staging area for new uploads |
| `attachments/` | General file attachments |
| `profiles/` | User profile images |
| `documents/` | Formal documents / evidence |
| `{entity}/` | Domain-specific prefixes as needed |

Use prefix-based isolation so IAM policies can scope access per folder if needed.

## Security Checklist

### Network

- [ ] MinIO is **not** publicly routable (firewall / security groups / private Docker network)
- [ ] Only the API can reach MinIO on port 9000
- [ ] TLS enabled in production (`WithSSL: true` + reverse proxy)

### Identity & Access

- [ ] Dedicated MinIO user with least-privilege policy (scoped to bucket/prefix)
- [ ] No root/admin keys in application config
- [ ] Credentials loaded from environment variables or secrets manager
- [ ] Rotate access keys periodically

### Bucket & Objects

- [ ] Bucket is **private** (no public read/write policy)
- [ ] Object keys generated server-side (`Guid` + extension)
- [ ] Validate MIME type and file size before upload
- [ ] Consider async malware scanning for production

### API Endpoints

- [ ] Protect upload/download/delete with authentication and authorization
- [ ] Use short expiry for pre-signed URLs (5–15 minutes)
- [ ] Never return MinIO credentials to clients
- [ ] Rate-limit upload endpoints

## Sequence Diagram

```
Client          API              MinIO           Database
  │              │                 │                 │
  │ POST /uploads│                 │                 │
  │─────────────►│ PutObject       │                 │
  │              │────────────────►│                 │
  │              │◄────────────────│                 │
  │◄─────────────│ filePath        │                 │
  │              │                 │                 │
  │ POST /entity │                 │                 │
  │ + path       │                 │                 │
  │─────────────►│ FileExists      │                 │
  │              │────────────────►│                 │
  │              │ MoveObject      │                 │
  │              │────────────────►│                 │
  │              │ Save path       │                 │
  │              │─────────────────────────────────►│
  │◄─────────────│ 201 Created     │                 │
```

## FAQ

### "Do we need a public MinIO URL?"

**No.** Clients upload to the API. The API uploads to MinIO on a private network.

### "Can the browser upload directly to MinIO?"

Not in this architecture. That would require pre-signed PUT URLs and exposing the MinIO endpoint to the browser. This pattern intentionally avoids that for uploads.

### "How do clients download files without knowing MinIO?"

Use the API proxy (`/files/download` or `/files/get-file`). The API fetches from MinIO and streams the response. The client only knows the API domain.

### "When should we use pre-signed URLs?"

Only for **reads** when you want the client to download directly from storage (e.g. large files, CDN-like behavior). Always generate them server-side after authorization. Never use them for uploads in this pattern.

### "What does the database store?"

Store the **object path** (e.g. `attachments/3f2a1b9c-....pdf`), not a full MinIO URL. The API resolves paths to objects at read time.

## Implementation Guide

1. Run MinIO on a private network (Docker Compose service or internal VPC)
2. Register `IMinioClient` in DI with server-side credentials
3. Create a storage service with upload, download, exists, move, and delete
4. Expose `POST /files/uploads` — return object path, not MinIO URL
5. In domain services, accept `path`, validate, move from `tmp/` to permanent prefix
6. Store object path in the database
7. Expose `GET /files/download` for reads through the API
8. Optionally expose `GET /files/url` for pre-signed read URLs
9. Protect all endpoints with authentication and authorization
10. Never make the MinIO bucket public

## Suggested API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `POST` | `/api/v1/files/uploads` | Stage a file upload |
| `POST` | `/api/v1/files/uploads-files` | Stage multiple files |
| `GET` | `/api/v1/files/download` | Stream file through API |
| `GET` | `/api/v1/files/url` | Get pre-signed read URL |
| `GET` | `/api/v1/files/get-file` | Public/anonymous file proxy |
| `GET` | `/api/v1/files/exists` | Check if object exists |
| `DELETE` | `/api/v1/files` | Delete an object |
| `POST` | `/api/v1/files/move` | Move object to permanent folder |

---

*Standard file storage pattern: API as gateway, MinIO as private object store.*
