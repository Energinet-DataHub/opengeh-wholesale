from functools import wraps
from opentelemetry.trace import SpanKind, Status, StatusCode, Span
import package.infrastructure.logging_configuration as config


def log_execution(
    cloud_role_name: str, applicationinsights_connection_string: str | None
):
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            config.configure_logging(
                cloud_role_name=cloud_role_name,
                applicationinsights_connection_string=applicationinsights_connection_string,
                extras={"Subsystem": "wholesale-aggregations"},
            )

            with config.get_tracer().start_as_current_span(
                func.__name__, kind=SpanKind.SERVER
            ) as span:
                try:
                    result = func(*args, **kwargs)
                    return result
                except SystemExit as e:
                    if e.code != 0:
                        record_exception(e, span)
                    raise
                except Exception as e:
                    record_exception(e, span)
                    raise

        return wrapper

    return decorator


def record_exception(exception: SystemExit | Exception, span: Span) -> None:
    span.set_status(Status(StatusCode.ERROR))
    span.record_exception(
        exception,
        attributes=config.get_extras()
        | {"CategoryName": f"Energinet.DataHub.{__name__}"},
    )
