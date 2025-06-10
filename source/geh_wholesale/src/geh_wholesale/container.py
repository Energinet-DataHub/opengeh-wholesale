from dependency_injector import containers, providers
from pyspark.sql import SparkSession

import geh_wholesale
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings


class Container(containers.DeclarativeContainer):
    infrastructure_settings = providers.Configuration()
    spark = providers.Factory(lambda: None)


def create_and_configure_container(
    spark: SparkSession,
    infrastructure_settings: InfrastructureSettings,
) -> Container:
    container = Container()

    container.spark.override(spark)

    container.infrastructure_settings.from_value(infrastructure_settings)

    container.wire(packages=[geh_wholesale])

    return container
