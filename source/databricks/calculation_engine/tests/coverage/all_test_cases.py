# fmt: off

from dataclasses import dataclass


@dataclass
class Cases:

    class CalculationTests:
        Typical_energy_scenario: str
        Typical_wholesale_scenario: str
        Calculation_results_are_hourly_when_calculation_period_is_before_result_resolution_change: str
        Calculation_input_data_includes_other_ga: str

        class MeteringPointMasterDataUpdates:
            Change_of_settlement_method_on_an_MP: str
            Change_of_resolution_on_an_MP: str
            Change_of_balance_responsible_on_an_MP: str
            Change_of_energy_supplier_on_an_MP: str
            Energy_Supplier_change_on_Grid_Loss_MP: str
            Energy_Supplier_change_on_System_Correction_MP: str
            Balance_Responsible_change_on_Grid_Loss_MP: str
            Balance_Responsible_change_on_System_Correction_MP: str
            MP_is_shut_down_in_calculation_period: str
            MP_is_starts_up_in_calculation_period: str

        class ExchangeCases:
            Exchange_between_two_ga_where_exchange_MP_is_in_neither_ga: str
            Exchange_MP_where_from_and_to_is_the_same_grid_area: str

        class UnusualGridAreaSetups:
            Grid_area_with_only_non_profiled_MP: str
            Grid_area_with_only_exchange_MP: str
            Grid_area_with_only_production_MP: str
            Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP: str

        class PriceElementUpdates:
            Extra_fees_are_added_to_MP_in_calculation_period: str
            Extra_subscriptions_are_added_to_MP_in_calculation_period: str

        class PriceElementsAndMPPeriods:
            Price_element_from_date_inside_calculation_period: str
            Price_element_to_date_inside_calculation_period: str
            No_price_points_for_active_price_elements_in_the_calculation_period: str

        class MultipleGridAreasInCalculations:
            Calculation_covers_multiple_grid_areas: str
            Calculation_includes_2_out_of_3_MP_grid_areas_in_input_data: str

    class DataProductTests:

        class WholesaleResultsTests:
            Only_calculation_ids_in_internal_calculations_included: str
            Only_external_calculation_ids_included: str
            Calculation_ids_without_calculation_succeeded_time_not_included: str
            Correct_mp_types_included_in_energy_v1_output: str

        class SapResultsTests:
            Calculation_history_splits_period_in_correct_amount_of_days: str
            Calculation_history_sets_correct_latest_calculation_when_periods_overlap: str
            Correct_calculation_ids_included_in_output: str
            Only_calculation_ids_with_succeeded_time_included: str

        class SettlementReportsTests:
            Charge_link_period_between_MP_periods: str
            Charge_link_period_is_a_subset_of_the_MP_period: str
            Charge_link_periods_have_a_gap: str
            Charge_link_periods_multiple: str
            Charge_link_period_missing: str
            Charge_link_period_between_MP_periods: str
            Charge_price_information_periods_multiple: str
            MP_period_missing: str
            MP_period_multiple: str
            MP_period_and_charge_link_period_do_not_overlap: str
            MP_period_and_charge_link_period_overlap: str
            MP_period_subset_of_charge_link_period: str
            MP_with_same_masterdata_but_different_to_and_from_only_one_entry_in_charge_prices: str
            Calculation_versions_different: str
            Calculation_types_different: str
