from pyspark.sql import DataFrame, functions as F, Window

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def merge_connected_periods(df: DataFrame) -> DataFrame:
    """
    Merges connected and/or overlapping periods within each group of rows in the input DataFrame.
    Args:
        df: a dataframe that contains any number of columns plus the columns 'from_date' and 'to_date'

    Returns:
        A DataFrame with the same columns as the input DataFrame.
        Rows that had overlapping/connected periods are merged into single rows.

    """
    other_columns = [
        col
        for col in df.columns
        if col not in [DataProductColumnNames.from_date, DataProductColumnNames.to_date]
    ]
    window_spec = Window.partitionBy(other_columns).orderBy(
        DataProductColumnNames.from_date
    )

    # Add columns to identify overlapping periods
    df_with_next = df.withColumn(
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
    other_columns.append("group")
    merged_df = df_with_group.groupBy(other_columns).agg(
        F.min(DataProductColumnNames.from_date).alias(DataProductColumnNames.from_date),
        F.max(DataProductColumnNames.to_date).alias(DataProductColumnNames.to_date),
    )

    return merged_df
