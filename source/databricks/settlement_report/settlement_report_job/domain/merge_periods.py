from pyspark.sql import DataFrame, functions as F, Window

from settlement_report_job.wholesale.column_names import DataProductColumnNames


def merge_connected_periods(
    input_df: DataFrame, group_by_columns: list[str]
) -> DataFrame:
    # Define a window specification to order by from_date
    window_spec = Window.partitionBy(group_by_columns).orderBy(
        DataProductColumnNames.from_date
    )

    # Add columns to identify overlapping periods
    df_with_next = input_df.withColumn(
        "next_from_date", F.lead(DataProductColumnNames.from_date).over(window_spec)
    ).withColumn(
        "next_to_date", F.lead(DataProductColumnNames.to_date).over(window_spec)
    )

    # Add a column to identify the start of a new group of connected periods
    df_with_group = df_with_next.withColumn(
        "group",
        F.sum(
            F.when(
                F.col(DataProductColumnNames.from_date)
                > F.lag(DataProductColumnNames.to_date).over(window_spec),
                1,
            ).otherwise(0)
        ).over(window_spec.rowsBetween(Window.unboundedPreceding, Window.currentRow)),
    )

    # Merge overlapping periods within each group
    group_by_columns.append("group")
    merged_df = df_with_group.groupBy(group_by_columns).agg(
        F.min(DataProductColumnNames.from_date).alias(DataProductColumnNames.from_date),
        F.max(DataProductColumnNames.to_date).alias(DataProductColumnNames.to_date),
    )

    return merged_df
