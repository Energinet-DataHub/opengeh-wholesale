from typing import Callable, Any, Tuple, Dict

from settlement_report_job.logging.logging_configuration import start_span
from settlement_report_job.logging import Logger


def use_span(name: str | None = None) -> Callable[..., Any]:
    """
    Decorator for creating spans.
    If name is not provided then the __qualname__ of the decorated function is used.
    """

    def decorator(func: Callable[..., Any]) -> Callable[..., Any]:
        def wrapper(*args: Tuple[Any], **kwargs: Dict[str, Any]) -> Any:
            name_to_use = name or func.__qualname__
            with start_span(name_to_use):
                log = Logger(name_to_use)
                log.info(f"Started executing function: {name_to_use}")
                return func(*args, **kwargs)

        return wrapper

    return decorator
