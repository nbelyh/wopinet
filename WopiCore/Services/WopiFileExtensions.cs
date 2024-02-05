using System;

namespace WopiCore.Services
{
    public static class WopiFileExtensions
    {
        public static bool IsLocked(this WopiFile file)
        {
            return file.LockValue != null && file.LockExpires > DateTime.UtcNow;
        }

        public static bool IsSameLock(this WopiFile file, string lockId)
        {
            return file.LockValue == lockId;
        }

        public static void Lock(this WopiFile file, string lockId, double lockDurationMinutes)
        {
            file.LockValue = lockId;
            file.LockExpires = DateTime.UtcNow.AddMinutes(lockDurationMinutes);
        }

        public static void Unlock(this WopiFile file)
        {
            file.LockValue = null;
        }


    }
}
