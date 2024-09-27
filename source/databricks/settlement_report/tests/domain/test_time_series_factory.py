from unittest.mock import Mock

from settlement_report_job.domain.time_series_factory import create_time_series


def test_create_time_series() -> None:
    # Arrange
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = sample_df
    resolution = DataProductMeteringPointResolution.HOUR

    # Act
    result_df = create_time_series(args, resolution, mock_repository)

    # Assert
    assert result_df is not None
    assert result_df.count() > 0
    # Add more assertions as needed to verify the correctness of the result_df
