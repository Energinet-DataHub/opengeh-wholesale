from typing import Callable, Any, Tuple, Dict

from settlement_report_job.logging.logging_configuration import start_span


def use_span(name: str | None = None) -> Callable[..., Any]:
    """
    Decorator for creating spans.
    If name is not provided then the __qualname__ of the decorated function is used.
    """

    def decorator(func: Callable[..., Any]) -> Callable[..., Any]:
        def wrapper(*args: Tuple[Any], **kwargs: Dict[str, Any]) -> Any:
            with start_span(name or func.__qualname__):
                return func(*args, **kwargs)

        return wrapper

    return decorator
