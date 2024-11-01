from opentelemetry.trace import Status, StatusCode, Span
import settlement_report_job.logging.logging_configuration as config


def span_record_exception(exception: SystemExit | Exception, span: Span) -> None:
    span.set_status(Status(StatusCode.ERROR))
    span.record_exception(
        exception,
        attributes=config.get_extras()
        | {"CategoryName": f"Energinet.DataHub.{__name__}"},
    )
