$ErrorActionPreference = 'Stop'

# Note: Github CLI is required to run this script, see https://cli.github.com/ for details.

gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/opengeh-revision-log
gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/opengeh-esett-exchange
gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/opengeh-grid-loss-imbalance-prices
gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/opengeh-migration
gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/dh3-operations
gh browse docs/diagrams/c4-model/model.dsl -R Energinet-DataHub/dh2-bridge

