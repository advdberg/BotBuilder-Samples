// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Controllers
{   
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly string _appPassword;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private ConnectorClient _conClient;

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _appPassword = configuration["MicrosoftAppPassword"] ?? string.Empty;
            MicrosoftAppCredentials.TrustServiceUrl("https://smba.trafficmanager.net/emea/");
            _conClient = new ConnectorClient(new Uri("https://smba.trafficmanager.net/emea/"), _appId, _appPassword);
        }

        [Route("api/notifyusers")]
        public async Task<IActionResult> Get()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            
            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        [Route("api/notifyusers/{userazureid?}")]
        public async Task<IActionResult> Get(Guid? userazureid)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                if (conversationReference.User.AadObjectId == userazureid.ToString())
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));

                    // Let the caller know proactive messages have been sent
                    return new ContentResult()
                    {
                        Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                        ContentType = "text/html",
                        StatusCode = (int)HttpStatusCode.OK,
                    };
                }                
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = string.Format("<html><body><h1>Proactive messages have NOT been sent, user with id {0} not found!</h1></body></html>",userazureid),
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.NotFound,
            };
        }

        [Route("api/notifyusers/chat/{chatthreadid}")]
        public async Task<IActionResult> Get(string chatthreadid)
        {
            try
            {
                await _conClient.Conversations.SendToConversationAsync(chatthreadid, MessageFactory.Text($"A message from me to you"));

                // Let the caller know proactive messages have been sent
                return new ContentResult()
                {
                    Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            catch (Exception e)
            {
                // Let the caller know proactive messages have been sent
                return new ContentResult()
                {
                    Content = "<html><body><h1>Proactive messages have NOT been sent.</h1></body></html>",
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
                      
        }

        [Route("api/notifyusers/chat/{chatthreadid}/{message}")]
        public async Task<IActionResult> Get(string chatthreadid, string message)
        {
            try
            {
                await _conClient.Conversations.SendToConversationAsync(chatthreadid, MessageFactory.Text(message));

                // Let the caller know proactive messages have been sent
                return new ContentResult()
                {
                    Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            catch (Exception e)
            {
                // Let the caller know proactive messages have been sent
                return new ContentResult()
                {
                    Content = "<html><body><h1>Proactive messages have NOT been sent.</h1></body></html>",
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }

        }
                
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync("proactive hello");
        }
    }
}
