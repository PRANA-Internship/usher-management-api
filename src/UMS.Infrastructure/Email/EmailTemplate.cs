using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Email
{

    public static class EmailTemplates
    {
        public static string ApplicationReceived(string fullName) => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#2c3e50">Application Received</h2>
              <p>Dear <strong>{fullName}</strong>,</p>
              <p>Thank you for submitting your usher application. We have received it successfully.</p>
              <p>Our team will review your application and you will be notified of the outcome.</p>
              <br/>
              <p>Best regards,<br/>UMS Team</p>
            </div>
            """;

        public static string PasswordSetup(string fullName, string setupUrl) => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#27ae60">Application Approved!</h2>
              <p>Dear <strong>{fullName}</strong>,</p>
              <p>Congratulations! Your usher application has been approved.</p>
              <p>Please click the button below to set your password and activate your account:</p>
              <div style="text-align:center;margin:30px 0">
                <a href="{setupUrl}"
                   style="background:#27ae60;color:#fff;padding:14px 28px;
                          text-decoration:none;border-radius:6px;font-size:16px">
                  Set Your Password
                </a>
              </div>
              <p style="color:#888;font-size:13px">
                This link expires in 48 hours. If you did not apply, ignore this email.
              </p>
              <br/>
              <p>Best regards,<br/>UMS Team</p>
            </div>
            """;
    }
}
