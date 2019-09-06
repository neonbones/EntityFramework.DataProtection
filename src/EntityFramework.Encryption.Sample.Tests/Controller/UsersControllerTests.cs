﻿using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Encryption.Sample.Controllers;
using EntityFramework.Encryption.Sample.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace EntityFramework.Encryption.Sample.Tests.Controller
{
    public class UsersControllerTests: IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _httpClient;
        
        public UsersControllerTests(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task Should_CreateUser_WithoutError()
        {
            var user = new UserCreateModel
            {
                Email = "email@email.com",
                PhoneNumber = "400000000",
                SensitiveData = "Some sensitive data"
            };

            var response = await _httpClient
                .PostAsync("api/users",
                    new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            
            var userId = await response
                .Content
                .ReadAsAsync<long>();

            var usersResponse = await _httpClient
                .GetAsync($"api/users/{userId}");
            
            usersResponse.EnsureSuccessStatusCode();

            var userFromDatabase = await usersResponse
                .Content
                .ReadAsAsync<UserDto>();
            
            Assert.NotNull(userFromDatabase);
            Assert.Equal(user.Email, userFromDatabase.PersonalData.Email);
            Assert.Equal(user.SensitiveData, userFromDatabase.PersonalData.SensitiveData);
            Assert.Equal(user.PhoneNumber, userFromDatabase.PersonalData.PhoneNumber);
        }
    }

    public class UserDto
    {
        public long Id { get; set; }

        public PersonalDataDto PersonalData { get; set; }
    }

    public class PersonalDataDto
    {
        public string PhoneNumber { get; set; }

        public string Email { get; set; }
        
        public string SensitiveData { get; set; }
        
        public string PhoneNumberHash { get; protected set; }
    }
}