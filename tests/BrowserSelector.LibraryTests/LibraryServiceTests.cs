using BrowserSelector.Library.Core.Services;
using BrowserSelector.Library.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BrowserSelector.LibraryTests;

public class LibraryServiceTests
{
    [Fact]
    public void LibraryService_GetLibraryMessage_ShouldReturnCorrectMessage()
    {
        // Arrange
        ILibraryService libraryService = new LibraryService();

        // Act
        string message = libraryService.GetLibraryMessage();

        // Assert
        message.Should().Be("Hello from BrowserSelector.Library.Infrastructure!");
    }
}