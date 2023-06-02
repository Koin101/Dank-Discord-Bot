using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Discord_Bot
{


    public class StableDiffusionApi
    {
        HttpClient client;

        bool busy;

        string webUiUrl = "http://127.0.0.1:7860/sdapi/v1/txt2img";



        public MemoryStream txt2imgRequest(Payload payload)
        {
            client = new HttpClient();

            try
            {
                var response = client.PostAsJsonAsync<Payload>(webUiUrl, payload);

                var r = response.Result.Content.ReadAsStringAsync().Result;

                ApiObject apiObject = JsonSerializer.Deserialize<ApiObject>(r);

                var bytes = Convert.FromBase64String(apiObject.images[0]);
                var contents = new MemoryStream(bytes);

                return contents;
            }
            catch (Exception e)
            {
                return null;
                
            }
            
        }
    }

    public class ApiObject
    {
        public string[] images { get; set; }
        public object parameters { get; set; }
        public object info { get; set; }
    }

    
    public class Payload
    {
        public bool enable_hr = false, restore_faces = false, tilting = false, do_not_save_samples = false, do_not_save_grid = false, send_images = true, save_images = false;
        public int denoising_strength = 0, firstphase_width = 0, firstphase_heigth = 0, hr_scale = 2, hr_second_pass_steps = 0, hr_resize_x = 0, hr_resize_y = 0,
            seed = -1, subseed = -1, subseed_strength = 0, seed_resize_from_h = -1, seed_resize_from_w = -1, batch_size = 1, n_iter = 1, steps = 50, cfg_scale = 7,
            width= 512, height = 512, eta = 0, s_min_uncond = 0, s_churn = 0, s_tmax = 0, s_tmin = 0, s_noise = 1;
        public string hr_upscaler = "", hr_sample_name = "", hr_prompt = "", hr_negative_prompt = "", prompt = "", sampler_name = "", negative_prompt = "", sampler_index = "Euler",
            script_name = "";
        public string [] styles;
        

        public Payload(string txtPrompt,  string negativePrompt, int width, int height, int seed, string samplerName)
        {
            prompt = txtPrompt;
            negative_prompt = negativePrompt;
            this.width = width;
            this.height = height;
            this.seed = seed;
            this.steps = 20;

        }


    }
}
