Idea
- Based on the chain of responsibility and decorator pattern
- Calculation results are store in CalculationOutput (the set method is only allowed once)
- CalculationOutput is passed down the chain

- Use DI where ever possible in links, services and repos
- Use a bucket for data that can be reused in other links


- Setup chain (validate)
- Prepare chain (fetch and wash data)
- Energy calculation chain
- Wholesale calculation chain
- Write to UC chain



Caching
- Add caching to bucket? To repositories? The point is to have caching one place only or at least limit it to one layer

Repositories
- Do the repos just have a get() and then use it by get().where(...).where() or get_by(grid_code_ids, etc.)
- One per area? One per table? One per entity?


Stack
- 3 Layers

Chain of Responsibility
-  The use od sub-chains might clarify logic fx. energy chain and wholesale chain and write to UC chain
- A calculation link must contribute to calculation output otherwise it's not a link? Or is a link just a piece of logic?
  It means that get all metering points isn't a link's job and the first link that needs metering points must fetch them
- Inject of bucket.grid_loss_metering_points? How to do that? Can you inject bucket.grid_loss_metering_points?
               
Minor
- Make a common static class for the DI validation in __init__
- A link can terminate the chain.
- A link can jump to 5 links ahead fx. it can jump to the write link
