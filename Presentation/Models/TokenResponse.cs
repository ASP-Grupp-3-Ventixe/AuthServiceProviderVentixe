﻿namespace Presentation.Models
{
    public class TokenResponse
    {
        public bool Succeeded { get; set; }
        public string? AccessToken { get; set; }
        public string? Message { get; set; }
    }
}
