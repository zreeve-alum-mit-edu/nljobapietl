using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobApi.Common;

public class JobContextFactory : IDesignTimeDbContextFactory<JobContext>
{
    public JobContext CreateDbContext(string[] args)
    {
        var connectionString = JobContext.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<JobContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

        return new JobContext(optionsBuilder.Options);
    }
}
