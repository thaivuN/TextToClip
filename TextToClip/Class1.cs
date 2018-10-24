using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sony.Vegas;

namespace TextToClip
{
    enum Preset
    {
        Rank, Title, Song
    }

    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            
            List<string> rawData = readFromFile();
            printList(rawData);

            

            PlugInNode node = vegas.Generators;
            //Load the selected Track
            Track track = getSelectedTrack(vegas.Project);
            //Load selected events from Track
            TrackEvent[] events = getSelectedEventsInTrack(track);

            if (events.Length != rawData.Count)
            {
                MessageBox.Show("The number of input and clips does not match");
                return;
            }

            //Get the Text media
            //Media txt_media = createTextMedia(vegas);

            //Create Ranking Track and Text events
            VideoTrack track1 = createTrackAbove(vegas.Project, track);
            VideoEvent[] rankTexts = createText(vegas,track1, events, Preset.Rank);
            //Create Title Track and Text events
            VideoTrack track2 = createTrackAbove(vegas.Project, track);
            VideoEvent[] titleTexts = createText(vegas,track2, events, Preset.Title);     
            //Create Songs Track and Text events
            VideoTrack track3 = createTrackAbove(vegas.Project, track);
            VideoEvent[] songTexts = createText(vegas, track3, events, Preset.Song);


        }

        private Track getSelectedTrack(Project project)
        {
            foreach (Track track in project.Tracks)
                if (track.Selected)
                    return track;
            return null;
        }

        /**
         * Fetch all the selected media in Sony Vegas
         * */
        private TrackEvent[] getSelectedEventsInTrack(Track track)
        {
            List<TrackEvent> events = new List<TrackEvent>();

            foreach(TrackEvent ev in track.Events)
                if (ev.Selected)
                    events.Add(ev);
            
            return events.ToArray();
        }

        private VideoTrack createTrackAbove(Project project, Track track)
        {
            VideoTrack tr = new VideoTrack(track.Index);
            project.Tracks.Add(tr);
            return tr;
        }

        private VideoEvent[] createText(Vegas vegas, VideoTrack track, TrackEvent[] eventsBelow, Preset preset)
        {
            List<VideoEvent> events = new List<VideoEvent>();
            
            foreach(TrackEvent subEvent in eventsBelow)
            {
                Media media = createTextMedia(vegas, preset);
                VideoEvent txt = createText(media, track, subEvent);
                events.Add(txt);
                OFXEffect ofxEffect = media.Generator.OFXEffect;
                OFXStringParameter tparam = (OFXStringParameter) ofxEffect.FindParameterByName("Text");

                string value = replaceString(tparam.Value, getDefaultString(preset),randomText());
                tparam.Value = value;
                //string rand = randomText();
            }
                

            return events.ToArray();
        }

        private VideoEvent createText(Media media, VideoTrack track, TrackEvent eventBelow)
        {
            VideoEvent txtEvent = track.AddVideoEvent(eventBelow.Start, eventBelow.End - eventBelow.Start);
            Take take = txtEvent.AddTake(media.GetVideoStreamByIndex(0));
            return txtEvent;
        }

        private Media createTextMedia(Vegas vegas, Preset preset)
        {
            PlugInNode plugin = vegas.Generators.GetChildByName("Sony Titles & Text");
            Media textMedia = new Media(plugin);

            switch (preset)
            {
                case Preset.Title:
                    textMedia.Generator.Preset = "Titles";
                    break;
                case Preset.Song:
                    textMedia.Generator.Preset = "Songs";
                    break;
                case Preset.Rank:
                    textMedia.Generator.Preset = "Rankings";
                    break;
                default:
                    break;
            }

            return textMedia;
        }

        private List<string> readFromFile()
        {
            List<string> lines = new List<string>();

            string path = "";

            using(OpenFileDialog openfile = new OpenFileDialog())
            {
                openfile.Filter = "txt files (*.txt)|*.txt";
                openfile.RestoreDirectory = true;
                if(openfile.ShowDialog() == DialogResult.OK)
                {
                    path = openfile.FileName;
                   
                }
            }

            if (System.IO.File.Exists(path))
            {
                string[] data = System.IO.File.ReadAllLines(path);
                lines = reverse(data);

            }

            return lines;
        }

        private List<string> reverse(string[] data)
        {
            Stack<string> s = new Stack<string>();
            List<string> revData = new List<string>();

            foreach(string row in data)
            {
                if (row.Trim().Length != 0)
                    revData.Add(row);
            }

            revData.Reverse();

            return revData;
            
        }

        private void printList(List<string> list)
        {
            string print = "The content of the list are: \n";
            foreach(string line in list)
            {
                print += line + "\n";
            }
            MessageBox.Show(print);
        }

        private string randomText()
        {
            string[] rands = { "Text 1", "Text 13q", "I hate you" };
            Random random = new Random();
            int index = random.Next(0, 3);
            return rands[index];
        }
        
        private string getDefaultString(Preset preset)
        {
            string origin = "";
            switch (preset)
            {
                case Preset.Rank:
                    origin = "Rankings";
                    break;
                case Preset.Song:
                    origin = "Songs";
                    break;
                case Preset.Title:
                    origin = "Titles";
                    break;
            }
            return origin;
        }

        private string replaceString(string origin, string oldVal, string newVal)
        {
            StringBuilder stringBuilder = new StringBuilder(origin);
            stringBuilder.Replace(oldVal, newVal);

            return stringBuilder.ToString();
        }

        private List<string> reEncode(List<string> data, string oldDelimiter, string newDelimiter)
        {
            List<string> encoded = new List<string>();

            foreach(string row in data){
                StringBuilder sb = new StringBuilder(row);
                sb.Replace(oldDelimiter, newDelimiter);

                encoded.Add(sb.ToString());
            }

            encoded.Reverse();

            return encoded;
        }
    }
}
