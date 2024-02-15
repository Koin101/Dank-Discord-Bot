using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI_API;


namespace Discord_Bot
{
    class OpenAI
    {
        

        OpenAIAPI api = new OpenAIAPI(Environment.GetEnvironmentVariable("OpenAIKey"));

        OpenAIService apiImage = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = Environment.GetEnvironmentVariable("OpenAIKey")
        });


        public async Task<string> Textrequest(string prompt, string model, string temp)
        {
            Model ChosenModel;
            if (model == "curie") ChosenModel = Model.CurieText;
            else if (model == "ada") ChosenModel = Model.AdaText;
            else ChosenModel = Model.DavinciText;

            
            var result =  await api.Completions.CreateCompletionAsync(new CompletionRequest(prompt, model: ChosenModel, temperature: Convert.ToDouble(temp), max_tokens:2000));

            return result.ToString();

        }

        public async Task<string> ImageRequest( string prompt)
        {
            var imageResult = await apiImage.Image.CreateImage(new ImageCreateRequest
            {
                Prompt = prompt,
                N = 1,
                //Size = StaticValues.ImageStatics.Size.Size512,
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = "public Discord bot"
            });

            if (imageResult.Successful) return String.Join("\n", imageResult.Results.Select(r => r.Url));
            else return "fail";
        }


    }
}
