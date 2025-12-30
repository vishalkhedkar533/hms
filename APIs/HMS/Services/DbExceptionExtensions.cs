using Microsoft.EntityFrameworkCore;

namespace HMS.Services
{
    public static class DbExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
        {
            var message = exception.InnerException?.Message?.ToLower() ?? "";

            return message.Contains("unique constraint")
                || message.Contains("duplicate key")
                || message.Contains("unique index")
                || message.Contains("duplicate entry");
        }
    }
}
