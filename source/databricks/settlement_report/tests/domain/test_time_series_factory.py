from datetime import datetime
from pyspark.sql import functions as F, types as T
from settlement_report_job.domain.time_series_factory import pad_array_col
from settlement_report_job.infrastructure.table_column_names import (
    EphemeralColumns,
    DataProductColumnNames,
)


def test_pad_array_col__returns_column_padded_with_null_observations(spark):
    # Arrange
    data = [
        (
            1,
            [
                {"observation_time": datetime(2020, 1, 1, 0, 0, 0), "quantity": 1.00},
                {"observation_time": datetime(2020, 1, 2, 0, 0, 0), "quantity": 1.00},
                {"observation_time": datetime(2020, 1, 3, 0, 0, 0), "quantity": 1.00},
            ],
        ),
        (
            2,
            [
                {"observation_time": datetime(2020, 1, 1, 0, 0, 0), "quantity": 1.00},
                {"observation_time": datetime(2020, 1, 2, 0, 0, 0), "quantity": 1.00},
            ],
        ),
        (3, [{"observation_time": datetime(2020, 1, 1, 0, 0, 0), "quantity": 1.00}]),
    ]

    df_schema = T.StructType(
        [
            T.StructField("id", T.IntegerType(), False),
            T.StructField(
                "value",
                T.ArrayType(
                    T.StructType(
                        [
                            T.StructField("observation_time", T.TimestampType(), False),
                            T.StructField("quantity", T.DoubleType(), True),
                        ]
                    )
                ),
                False,
            ),
        ]
    )

    df = spark.createDataFrame(data, df_schema)

    # Act
    padded = (
        df.select(
            "id",
            "value",
            pad_array_col("value", 5, "QUANTITY").alias("padded_value"),
        )
        .select("id", "padded_value", F.size("padded_value").alias("size"))
        .collect()
    )

    # Assert
    sizes = [r.size for r in padded]
    non_null = [
        sum(
            [
                1
                for x in r.padded_value
                if x[DataProductColumnNames.quantity] is not None
            ]
        )
        for r in padded
    ]
    starts_with_quantity = [
        sum(
            [
                1
                for x in r.padded_value
                if x[EphemeralColumns.uid].startswith("QUANTITY")
            ]
        )
        for r in padded
    ]
    assert sizes == [5, 5, 5]
    assert non_null == [3, 2, 1]
    assert starts_with_quantity == [5, 5, 5]
