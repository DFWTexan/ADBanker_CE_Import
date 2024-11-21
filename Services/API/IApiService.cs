﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import.Services
{
    public interface IApiService
    {
        public Task<string> GetAccessToken();
        public Task<dynamic> GetCompletedCourses(string? accessToken, int? page, string? tags);
    }
}
