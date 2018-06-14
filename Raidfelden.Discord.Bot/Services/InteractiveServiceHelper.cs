using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Services
{
    public static class InteractiveServiceHelper
    {
        public static async Task<ServiceResponse<TEntity>> GenericGetEntityWithCallback<TEntity, TIdentifier>(Task<List<TEntity>> source, Func<List<TEntity>, List<TEntity>> findExactMatch, int interactiveLimit, Func<TIdentifier, Task<ServiceResponse>> interactiveCallback, Func<TEntity, TIdentifier> getEntityIdentifier, Func<TEntity, List<TEntity>, Task<string>> getEntityName, Func<TEntity, string> getSuccessMessage, Func<string> getErrorMessageNoEntityFound, Func<List<TEntity>, string> getErrorMessageInteractiveLimit, Func<List<TEntity>, string> getErrorMessageInteractive)
        {
            var entities = await source;
            // Happy path, only one entity found
            if (entities.Count == 1)
            {
                var entity = entities[0];
                return new ServiceResponse<TEntity>(true, getSuccessMessage(entity), entity);
            }

            // Check for NoEntityFound error
            if (entities.Count == 0)
            {
                return new ServiceResponse<TEntity>(false, getErrorMessageNoEntityFound(), default(TEntity));
            }

            // Check for exact match
            var entitesExact = findExactMatch(entities);
            if (entitesExact.Count == 1)
            {
                var entity = entitesExact[0];
                return new ServiceResponse<TEntity>(true, getSuccessMessage(entity), entity);
            }

	        var entitiesForCallback = entities;
	        if (entitesExact.Count > 0)
	        {
		        entitiesForCallback = entitesExact;
	        }
            return await GenericCreateCallbackAsync(interactiveLimit, interactiveCallback, getEntityIdentifier, getEntityName, getErrorMessageInteractiveLimit, getErrorMessageInteractive, entitiesForCallback);
        }

	    public static async Task<ServiceResponse<TEntity>> GenericCreateCallbackAsync<TEntity, TIdentifier>(int interactiveLimit, Func<TIdentifier, Task<ServiceResponse>> interactiveCallback,
		    Func<TEntity, TIdentifier> getEntityIdentifier, Func<TEntity, List<TEntity>, Task<string>> getEntityName, Func<List<TEntity>, string> getErrorMessageInteractiveLimit, Func<List<TEntity>, string> getErrorMessageInteractive,
		    List<TEntity> entities)
	    {
			// Check if we can use the interactive Mode
		    if (entities.Count > interactiveLimit)
		    {
			    return new ServiceResponse<TEntity>(false, getErrorMessageInteractiveLimit(entities), default(TEntity));
		    }

		    // Add the callback actions to the interactive Mode
		    var callbacks = new Dictionary<string, Func<Task<ServiceResponse>>>(entities.Count);
		    foreach (var entity in entities)
		    {
			    callbacks.Add(await getEntityName(entity, entities), () => interactiveCallback(getEntityIdentifier(entity)));
		    }

		    return new ServiceResponse<TEntity>(false, getErrorMessageInteractive(entities), default(TEntity), callbacks);
	    }
    }
}
