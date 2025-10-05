using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace TaskManagementSystem.Common.Extensions;

public class InMemoryRowVersionGenerator : ValueGenerator<byte[]>
{
    public override byte[] Next(EntityEntry entry)
    {
        return BitConverter.GetBytes(DateTime.UtcNow.Ticks);
    }

    public override bool GeneratesTemporaryValues => false;
}
