using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dappery.Core.Data;
using Dappery.Domain.Entities;
using Dappery.Domain.Media;
using Dappery.Core.Exceptions;
using MediatR;
using Dappery.Core.Extensions;

namespace Dappery.Core.Beers.Commands.CreateBeer
{
    public class CreateBeerCommandHandler : IRequestHandler<CreateBeerCommand, BeerResource>
    {
        private readonly IUnitOfWork unitOfWork;

        public CreateBeerCommandHandler(IUnitOfWork unitOfWork) => this.unitOfWork = unitOfWork;

        public async Task<BeerResource> Handle(CreateBeerCommand request, CancellationToken cancellationToken)
        {
            // Check to make sure the brewery exists from the given brewery ID on the request
            var existingBrewery = await this.unitOfWork.BreweryRepository.GetBreweryById(request.Dto.BreweryId, cancellationToken).ConfigureAwait(false);

            // Invalidate the request if no corresponding brewery exists
            // Since we're not overloading the '==' operator, let's use the 'is' comparison here
            if (existingBrewery is null)
            {
                throw new DapperyApiException($"Cannot create beer with brewery ID {request.Dto.BreweryId} as that brewery does not exist", HttpStatusCode.BadRequest);
            }

            // Attempt to parse the incoming BeerStyle enumeration value
            var parsedBeerStyle = Enum.TryParse(request.Dto.Style, true, out BeerStyle beerStyle);

            // Let's instantiate a beer instance
            var beerToAdd = new Beer
            {
                Name = request.Dto.Name,
                BeerStyle = parsedBeerStyle ? beerStyle : BeerStyle.Other,
                BreweryId = request.Dto.BreweryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add the record to the database and retrieve the record after we create it
            var createdBeerId = await this.unitOfWork.BeerRepository.CreateBeerAsync(beerToAdd, cancellationToken).ConfigureAwait(false);
            var createdBeer = await this.unitOfWork.BeerRepository.GetBeerByIdAsync(createdBeerId, cancellationToken).ConfigureAwait(false);
            this.unitOfWork.Commit();

            // Return the mapped beer
            return new BeerResource(createdBeer.ToBeerDto());
        }
    }
}
