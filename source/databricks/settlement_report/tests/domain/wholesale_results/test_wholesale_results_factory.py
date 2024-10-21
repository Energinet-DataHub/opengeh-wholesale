from unittest.mock import Mock

from pyspark.sql import SparkSession

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.wholesale_results.wholesale_results_factory import create_wholesale_results


def test_create_wholesale_results(spark: SparkSession) -> None:
    args = Mock(spec=SettlementReportArgs)
    df = .create(
        spark,
        default_data.create_time_series_data_spec(
            from_date=data_from_date, to_date=data_to_date
        ),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df
    actual = create_wholesale_results(args, repository)
