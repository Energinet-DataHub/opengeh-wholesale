import unittest
import uuid
from unittest.mock import MagicMock, patch
from pyspark.sql import SparkSession
from datetime import datetime, timedelta
from decimal import Decimal

from package.codelists import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.wholesale_results.wholesale_results_factory import (
    create_wholesale_results,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from pyspark.sql import DataFrame
import pyspark.sql.functions as F

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = datetime(2023, 1, 1)
DEFAULT_TO_DATE = datetime(2023, 1, 31)


class TestCreateWholesaleResults(unittest.TestCase):

    @classmethod
    def setUpClass(cls):
        cls.spark = (
            SparkSession.builder.master("local[1]").appName("Test").getOrCreate()
        )

    @classmethod
    def tearDownClass(cls):
        cls.spark.stop()

    def setUp(self):
        self.args = SettlementReportArgs(
            report_id="test_report",
            period_start=DEFAULT_FROM_DATE,
            period_end=DEFAULT_TO_DATE,
            calculation_type=CalculationType.WHOLESALE_FIXING,
            requesting_actor_market_role=MarketRole.GRID_ACCESS_PROVIDER,
            requesting_actor_id="test_actor_id",
            calculation_id_by_grid_area={"test_area": uuid.uuid4()},
            grid_area_codes=["test_area_code"],
            energy_supplier_ids=["test_supplier_id"],
            split_report_by_grid_area=False,
            prevent_large_text_files=False,
            time_zone=DEFAULT_TIME_ZONE,
            catalog_name="test_catalog",
            settlement_reports_output_path="test_path",
            include_basis_data=False,
            locale="en_US",
        )
        self.repository = MagicMock(spec=WholesaleRepository)

    def test_create_wholesale_results(self):
        # Mock the read_and_filter_from_view and prepare_for_csv functions
        with patch(
            "settlement_report_job.domain.wholesale_results.read_and_filter.read_and_filter_from_view"
        ) as mock_read_and_filter, patch(
            "settlement_report_job.domain.wholesale_results.prepare_for_csv"
        ) as mock_prepare_for_csv:
            # Create a sample DataFrame to return from the mock
            sample_data = [("example", 1)]
            sample_df = self.spark.createDataFrame(sample_data, ["column1", "column2"])

            mock_read_and_filter.return_value = sample_df
            mock_prepare_for_csv.return_value = sample_df

            # Call the function under test
            result = create_wholesale_results(self.args, self.repository)

            # Assertions
            mock_read_and_filter.assert_called_once_with(self.args, self.repository)
            mock_prepare_for_csv.assert_called_once_with(sample_df)
            self.assertIsInstance(result, DataFrame)
            self.assertEqual(result.collect(), sample_df.collect())


if __name__ == "__main__":
    unittest.main()
