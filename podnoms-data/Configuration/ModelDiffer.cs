using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace PodNoms.Data.Configuration {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Workaround")]
    public class ModelDiffer : MigrationsModelDiffer {
        public ModelDiffer(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotations,
            IChangeDetector changeDetector,
            IUpdateAdapterFactory updateAdapterFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies) :
            base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies) {
        }
        //FIXME Remove when https://github.com/aspnet/EntityFrameworkCore/issues/18943 is deployed
        public override IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target) {
            return base.GetDifferences(source, target)
                    .Where(o => !(o is UpdateDataOperation))
                    .ToList();
        }
    }
}
