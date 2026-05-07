namespace UMS.tests
{
    public class UnitTest1
    {

        [Fact]
        public void Addition_Should_Return_Correct_Result()
        {
            var a = 2;
            var b = 3;

            var result = a + b;

            Assert.Equal(5, result);

        }
    }
}
