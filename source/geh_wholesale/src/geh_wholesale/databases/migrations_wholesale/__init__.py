"""Package `input`.

Responsible for reading calculation input data from Delta tables.
Schemas are checked when reading.
Only minor transformations to data frames are done merely compensating
for minor inappropriatenesses in the input table formats.
"""

from .measurements_repository import MeasurementsRepository as MeasurementsRepository
from .repository import MigrationsWholesaleRepository as MigrationsWholesaleRepository
