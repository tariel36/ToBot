// The MIT License (MIT)
//
// Copyright (c) 2022 tariel36
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using ToBot.Communication.Messaging.Providers;

namespace ToBot.Discord
{
    public class DiscordEmotesProvider
        : IEmoteProvider
    {
        public string SleepingAccommodation { get { return ":sleeping_accommodation:"; } }

        public string KissingHeart { get { return ":kissing_heart:"; } }

        public string Blush { get { return ":blush:"; } }

        public string Smile { get { return ":smile:"; } }

        public string Laughing { get { return ":laughing:"; } }

        public string SlightFrown { get { return ":slight_frown:"; } }

        public string StuckOutTongueWinkingEeye { get { return ":stuck_out_tongue_winking_eye:"; } }

        public string Skull { get { return ":skull:"; } }

        public string Dagger { get { return ":dagger:"; } }

        public string Ambulance { get { return ":ambulance:"; } }

        public string HatchedChick { get { return ":hatched_chick:"; } }

        public string Sleeping { get { return ":sleeping:"; } }

        public string Star { get { return ":star:"; } }

        public string CustomEmote(string emote)
        {
            return $":{emote}:";
        }
    }
}
