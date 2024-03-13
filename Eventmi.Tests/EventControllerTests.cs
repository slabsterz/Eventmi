namespace Eventmi.Tests
{
    public class EventControllerTests
    {
        private RestClient _client;
        private const string baseUrl = @"https://localhost:7236/";

        // Create dbcontext
        // private static int addedEventId;

        [SetUp]
        public void Setup()
        {
            this._client = new RestClient(baseUrl);
        }

        private bool CheckEventExists(string eventName)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-EOQ527A\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;

            using (var context = new EventmiContext(options))
            {
                return context.Events.Any( x => x.Name == eventName);
            }
        }

        private async Task<Event> GetEventByIdAsync(int id)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-EOQ527A\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;

            using (var context = new EventmiContext(options))
            {
                return await context.Events.FirstOrDefaultAsync( x => x.Id == id);
            }
        }

        [Test]
        public async Task GetAllEvents_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            var request = new RestRequest("/Event/All", Method.Get);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Add_GetRequest_ReturnsAddView()
        {
            // Arrange
            var request = new RestRequest("/Event/Add", Method.Get);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Add_PostRequest_AddsEventAndRedirects()
        {
            // Arrange
            var newEvent = new EventFormModel
            {
                Name = "Test Event 10",
                Start = new DateTime(2024, 5, 2, 09, 0, 0),
                End = new DateTime(2024, 5, 2, 12, 0, 0),
                Place = "Somewhere"
            };

            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Name", newEvent.Name);
            request.AddParameter("Start", newEvent.Start);
            request.AddParameter("End", newEvent.End);
            request.AddParameter("Place", newEvent.Place);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));           
            Assert.That(CheckEventExists(newEvent.Name), Is.True);

        }

        [Test]
        public async Task Get_GetEventDetails_ReturnsSuccessAndValidEventDetails()
        {
            // Arrange
            int eventId = 8;
            var request = new RestRequest($"/Event/Details/{eventId}", Method.Get);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_GetEventDetails_ReturnsNotFound_WhenGivenInvalidEventId()
        {
            // Arrange
            int eventId = 0;
            var request = new RestRequest($"/Event/Details/{eventId}", Method.Get);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task EditAction_ReturnsViewForValidId()
        {
            // Arrange
            int eventId = 8;
            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Get);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task EditAction_EditEvent_EditsEventAndRedirects()
        {
            // Arrange
            int eventId = 8;
            var eventToEdit = await GetEventByIdAsync(eventId);

            var eventModel = new EventFormModel
            {
                Id = eventToEdit.Id,
                Name = eventToEdit.Name,
                Start = eventToEdit.Start,
                End = eventToEdit.End,
                Place = eventToEdit.Place
            };

            string updatedName = "NEW NAME HERE";
            eventModel.Name = updatedName;

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", eventModel.Id);
            request.AddParameter("Name", eventModel.Name);
            request.AddParameter("Start", eventModel.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", eventModel.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", eventModel.Place);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task EditAction_Post_WithIdMismatch_ReturnsNotFound()
        {
            // Arrange
            var eventId = 8;
            var eventToEdit = await GetEventByIdAsync(eventId);

            var eventModel = new EventFormModel
            {
                Id = 100,
                Name = eventToEdit.Name + "NEW",
                Start = eventToEdit.Start,
                End = eventToEdit.End,
                Place = eventToEdit.Place
            };                      

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", eventModel.Id);
            request.AddParameter("Name", eventModel.Name);
            request.AddParameter("Start", eventModel.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", eventModel.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", eventModel.Place);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task EditAction_Post_InvalidModelInput_ReturnsSuccessStatusCode()
        {
            // Arrange
            int eventId = 30;

            var eventModel = new EventFormModel
            {
                Id = eventId,
                Name = string.Empty
            };

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", eventModel.Id);
            request.AddParameter("Name", eventModel.Name);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task DeleteAction_Post_RedirectsToAllEvents()
        {
            // Arrange
            int eventId = 12; 

            var request = new RestRequest($"/Event/Delete/{eventId}", Method.Post);

            // Act
            var response = await this._client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}