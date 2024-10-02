        # TODO AJW:
        # - Based on the chain of responsibility and decorator pattern
        # - Calculation results are store in CalculationOutput (the set method is only allowed once)
        # - CalculationOutput is passed down the chain
        # - Use DI where ever possible in links, services and repos
        # - Use a bucket for data that can be reused in other links
        # - Add caching to bucket? To repositories? The point is to have caching one place only or at least define it to one layer
        # - 3 Layers
        # - The use od sub-chains might clarify logic fx. energy chain and wholesale chain and write to UC chain
        # - A calculation link must contribute to calculation output otherwise its not a link? Or is a link just a piece of logic?
        #   It means that get all metering points isn't a link's job and the first link that needs metering points must fetch them
        # - Inject of bucket.grid_loss_metering_points? How to do that? Can you inject bucket.grid_loss_metering_points?
               
        # Minor
        # - Make a common static class for the DI validation in __init__
        # - A link can terminate the chain.
        # - A link can jump to 5 links ahead fx. it can jump to the write link
