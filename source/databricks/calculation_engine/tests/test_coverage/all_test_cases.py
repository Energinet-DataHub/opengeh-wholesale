# fmt: off

from dataclasses import dataclass


@dataclass
class Tests:

    class CalculationTests:
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

            # hmm?
            Metering_point_used_as_input_to_wholesale_results_changes_energy_supplier: str

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
            When_from_date_for_a_price_element_in_charge_master_data_is_in_calculation_period: str
            When_to_date_for_a_price_element_in_charge_master_data_is_in_calculation_period: str
            Active_price_elements_and_charge_links_in_calculation_period_no_price_points_in_the_calculation_period: str

        class MultipleGridAreasInCalculations:
            Calculation_covers_multiple_grid_areas: str
            Calculation_includes_2_out_of_3_MP_grid_areas_in_input_data: str

    class SettlementReportsTests:
        Missing_metering_point_period: str
        Missing_charge_link_period: str
        two_metering_point_periods_due_to_a_change_of_energy_supplier_results: str
        Charge_link_periods_have_a_gap: str
        Two_charge_link_periods: str
        There_is_charge_link_period_between_MP_periods: str
        There_are_multiple_metering_point_periods_due_to_change_of_energy_supplier: str
        There_are_two_charge_price_information_periods: str
        There_are_two_metering_point_periods_due_to_a_change_of_balance_responsible: str
        There_is_a_combination_of_different_calculation_types: str
        There_are_different_calculation_versions: str

        class OverlapScenarios:
            MP_period_and_charge_link_period_overlap: str
            MP_period_and_charge_link_period_overlap: str
            MP_period_and_charge_link_period_do_not_overlap: str
            MP_period_is_a_subset_of_charge_link_period: str
            MP_periods_are_overlapping_a_charge_link_period: str
            Charge_link_period_is_a_subset_of_the_MP_period: str
