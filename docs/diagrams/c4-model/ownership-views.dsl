# The 'ownership-views.dsl' file is intended as a mean for viewing the ownership
# of model elements, e.g. which team owns a given "application".
# It should
#   * Extend the base model and override the 'dh3' software system
#   * Include of the `model.dsl` files from each domain repository using an URL

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/docs/diagrams/c4-model/dh-base-model.dsl {

    model {
        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {

            # IMPORTANT:
            # The order by which models are included is important for how the domain-to-domain relationships are specified.
            # A domain-to-domain relationship should be specified in the "client" of a "client->server" dependency, and
            # hence domains that doesn't depend on others, should be listed first.

            # Include Market Participant model
            !include https://raw.githubusercontent.com/Energinet-DataHub/geh-market-participant/main/docs/diagrams/c4-model/model.dsl

            # Include EDI model
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-edi/main/docs/diagrams/c4-model/model.dsl

            # Include Wholesale model
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-wholesale/main/docs/diagrams/c4-model/model.dsl

            # Include Frontend model
            !include https://raw.githubusercontent.com/Energinet-DataHub/greenforce-frontend/main/docs/diagrams/c4-model/model.dsl

            # Include Esett Exchange model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-esett-exchange/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65SDJSYCBIOHJXLR6RAZKXGYMQ

            # Include Migration model - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/opengeh-migration/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65TLE2ZKXTY6OGZUTD4ZKXGYEA

            # Include platform tools - requires a token because its located in a private repository
            # Token is automatically appended in "Raw" view of the file
            !include https://raw.githubusercontent.com/Energinet-DataHub/dh3-operations/main/docs/diagrams/c4-model/model.dsl?token=GHSAT0AAAAAACIBG65TMZNGUC55Z2NFSET4ZKXGWOA
        }
    }

    views {
        container dh3 "Volt" {
            title "Owned by Volt"
            description ""
            include "element.tag==Volt"
            exclude "* -> *"
        }
        container dh3 "Mandalorian" {
            title "Owned by Mandalorian"
            description ""
            include "element.tag==Mandalorian"
            exclude "* -> *"
        }
        container dh3 "Mosaic" {
            title "Owned by Mosaic"
            description ""
            include "element.tag==Mosaic"
            exclude "* -> *"
        }
        container dh3 "Titans" {
            title "Owned by Titans"
            description ""
            include "element.tag==Titans"
            exclude "* -> *"
        }
        container dh3 "Outlaws" {
            title "Owned by Outlaws"
            description ""
            include "element.tag==Outlaws"
            exclude "* -> *"
        }
        container dh3 "UIUXGuild" {
            title "Owned by UI/UX Guild"
            description ""
            include "element.tag==UI/UX Guild"
            exclude "* -> *"
        }
    }
}
