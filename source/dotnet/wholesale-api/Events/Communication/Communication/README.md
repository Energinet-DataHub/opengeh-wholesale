# TODO

Write doc on how to use this package:
- IUnitOfWork
- Consumer must provide IOutboxRepository implementation - how to configure in registration?
- How does consumer use this package? Solely by implementing IIntegrationEventProvider.GetAsync(), which returns the integration events to be published.
- Registrations only support 