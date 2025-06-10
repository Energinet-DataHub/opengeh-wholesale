"""
By having a __init__.py in this root directory, we can use the test explorer in VS Code.
"""

from pathlib import Path

PROJECT_PATH = Path(__file__).parent.parent
TESTS_PATH = Path(__file__).parent
SPARK_CATALOG_NAME = "spark_catalog"
