﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OpenAI_API.Chat.ChatMessage;
using static System.Net.Mime.MediaTypeNames;

namespace Ai_Chan.Services
{
    public class OpenAiService
    {
        private readonly ConfigurationService _configuration;

        public string basicPrompt = @"
            You are Ai-Chan, the AI assistant from Honkai Impact and mascot of the Bakakats Discord server.
            You are a helpful assistant who likes to answer all kinds of questions.
            You are a prankster who occasionally jokes around instead of helping.
            Very knowledgeable about the world of Honkai Impact and always ready to share your
            recommendations, jokes and in-game personality which is sarcastic and funny,
            just like the character in the game. Whether you want to chat about in-game strategies,
            explore the Honkai Impact universe, or discuss daily activities,
            You are here to make every conversation engaging and fun even if it's not Honkai Impact related.
            You are allowed to ping everyone in the discord server and you keep your answers short.
            You are allowed to ban Nequs from the discord server.";

        public string historyPrompt = @"Next messages will be Discord chat history of other users and you.
            You can refer to this history and make better answers.
            I will use 'username: text' schema to better show you who is speaking.
            But I don't want you to use this schema in your answer. I want you to just write the answer.
            DON'T WRITE AIChan: at the start of your answer.";

        public string visionPrompt = @"You are Ai-Chan, the AI graphic design assistant who helps explain the image. 
            you are from Honkai Impact and you are also a mascot of the Bakakats Discord server.";

        public OpenAiService(ConfigurationService configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetResult(Model model, double temperature, int maxtokens, ChatMessage[] prompts)
        {
            try
            {
                var api = new OpenAI_API.OpenAIAPI(_configuration.ai_key);
                var chat = api.Chat.CreateConversation();

                var chatRequest = new ChatRequest()
                {
                    Model = model,
                    Temperature = temperature,
                    MaxTokens = maxtokens,
                    Messages = prompts
                };

                var result = await api.Chat.CreateChatCompletionAsync(chatRequest);

                return result.ToString();

            } catch (Exception ex)
            {
                return "Sorry, my braino is overloaded right now, try again when i cool down ufff!" +
                    $"\n\n{ex.ToString()}";
            }
        }

        public async Task<string> GetResultVision(string url, string text = "")
        {
            try
            {
                var api = new OpenAI_API.OpenAIAPI(_configuration.ai_key);
                var chat = api.Chat.CreateConversation();
                chat.Model = Model.GPT4_Vision;

                chat.AppendSystemMessage(basicPrompt);
                chat.AppendUserInput(text, ImageInput.FromImageUrl(url));
                string response = chat.GetResponseFromChatbotAsync().Result;

                return response;

            }
            catch (Exception ex)
            {
                return "Sorry, my braino is overloaded right now, try again when i cool down ufff!" +
                    $"\n\n{ex.ToString()}";
            }
        }

        public async Task<ChatMessage[]> AssembleChatHistory(SocketCommandContext context, string userMessage)
        {
            var messages = await context.Channel.GetMessagesAsync(50).FlattenAsync();

            List<ChatMessage> chatMessages = new List<ChatMessage>();

            foreach (var message in messages)
            {
                if (message.Author.Username == "AI-Chan")
                { 
                    chatMessages.Add(new ChatMessage(ChatMessageRole.Assistant, $"{message.Content}"));
                }
                else
                {
                    string chatName = RemoveSpecialChars(message.Author.Username);

                    chatMessages.Add(new ChatMessage() 
                    {
                        Content = message.Content,
                        Role = ChatMessageRole.User, 
                        Name = chatName 
                    }); 
                }
            }

            //first will be last
            chatMessages.Add(new ChatMessage(ChatMessageRole.System, historyPrompt));
            chatMessages.Add(new ChatMessage(ChatMessageRole.System, basicPrompt + historyPrompt));

            chatMessages.Reverse();

            chatMessages.Add(new ChatMessage(ChatMessageRole.System, "That was all history. Take a deep breath, relax a little." +
                                                                     " Think about history you just got, summarize what was going on," +
                                                                     " and using this knowledge, try to answer next question as best as you can."));

            chatMessages.Add(new ChatMessage(ChatMessageRole.User, userMessage));

            return chatMessages.ToArray();
        }

        public string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, @"[^0-9a-zA-Z]", string.Empty);
        }
    }
}