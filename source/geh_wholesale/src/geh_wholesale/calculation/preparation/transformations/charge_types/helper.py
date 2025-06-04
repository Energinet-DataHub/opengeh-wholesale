from pyspark.sql.dataframe import DataFrame

from geh_wholesale.constants import Colname


def join_charge_price_information_and_charge_price(
    charge_price_information: DataFrame, charge_prices: DataFrame
) -> DataFrame:
    charge_price_information_with_prices = (
        charge_price_information.join(
            charge_prices,
            [
                charge_prices[Colname.charge_key] == charge_price_information[Colname.charge_key],
                charge_prices[Colname.charge_time] >= charge_price_information[Colname.from_date],
                charge_prices[Colname.charge_time] < charge_price_information[Colname.to_date],
            ],
            "inner",
        )
        .distinct()
        .select(
            charge_price_information[Colname.charge_key],
            charge_price_information[Colname.charge_code],
            charge_price_information[Colname.charge_type],
            charge_price_information[Colname.charge_owner],
            charge_price_information[Colname.charge_tax],
            charge_price_information[Colname.resolution],
            charge_price_information[Colname.from_date],
            charge_price_information[Colname.to_date],
            charge_prices[Colname.charge_time],
            charge_prices[Colname.charge_price],
        )
    )
    return charge_price_information_with_prices
