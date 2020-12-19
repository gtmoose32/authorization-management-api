using AuthorizationManagement.Api.Models.Internal;
using AutoFixture;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;

// ReSharper disable PossibleMultipleEnumeration

namespace AuthorizationManagement.Api.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public abstract class ControllerTestBase
    {
        protected Container Container { get; }
        protected IFixture Fixture { get; }
        protected IMapper Mapper { get; }

        protected ControllerTestBase()
        {
            Container = Substitute.For<Container>();
            Fixture = new Fixture();
            Mapper = Substitute.For<IMapper>();
        }
        
        protected static FeedIterator<T> CreateFeedIteratorWithResponse<T>(IEnumerable<T> results)
            where T : class
        {
            var feedIterator = Substitute.For<FeedIterator<T>>();
            feedIterator.HasMoreResults.Returns(true, false);

            var feedResponse = Substitute.For<FeedResponse<T>>();
            feedResponse.Resource.Returns(results);
            feedResponse.GetEnumerator().Returns(results.GetEnumerator());
            feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

            return feedIterator;
        }
        
        protected static ItemResponse<T> CreateItemResponse<T>(HttpStatusCode statusCode, T item, Headers headers = null)
            where T : class, IDocument
        {
            var args = new object[] { statusCode, headers, item, null };

            var type = typeof(ItemResponse<T>);
            var instance = type.Assembly.CreateInstance(
                // ReSharper disable once AssignNullToNotNullAttribute
                type.FullName, false, BindingFlags.Instance | BindingFlags.NonPublic, null, args, CultureInfo.InvariantCulture, null);

            return (ItemResponse<T>)instance;
        }
    }
}