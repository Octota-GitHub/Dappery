using System;
using System.Threading;
using System.Threading.Tasks;

using Dappery.Core.Data;
using Dappery.Core.Extensions;
using Dappery.Domain.Entities;
using Dappery.Domain.Media;

using MediatR;

namespace Dappery.Core.Breweries.Commands.CreateBrewery;

public class CreateBreweryCommandHandler : IRequestHandler<CreateBreweryCommand, BreweryResource>
{
    private readonly IUnitOfWork unitOfWork;

    public CreateBreweryCommandHandler(IUnitOfWork unitOfWork) => this.unitOfWork = unitOfWork;

    public async Task<BreweryResource> Handle(CreateBreweryCommand request, CancellationToken cancellationToken)
    {
        var breweryToCreate = new Brewery
        {
            Name = request.Dto.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Address = new Address
            {
                StreetAddress = request.Dto.Address?.StreetAddress,
                City = request.Dto.Address?.City,
                State = request.Dto.Address?.State,
                ZipCode = request.Dto.Address?.ZipCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Create our brewery, retrieve the brewery so we can map it to the response, and clean up our resources
        var breweryId = await this.unitOfWork.BreweryRepository.CreateBrewery(breweryToCreate, cancellationToken).ConfigureAwait(false);
        var insertedBrewery = await this.unitOfWork.BreweryRepository.GetBreweryById(breweryId, cancellationToken).ConfigureAwait(false);
        this.unitOfWork.Commit();

        // Map and return the response
        return new BreweryResource(insertedBrewery!.ToBreweryDto());
    }
}
