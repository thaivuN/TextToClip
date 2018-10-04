using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            PlugInNode node = vegas.Generators;
            //Load the selected Track
            Track track = getSelectedTrack(vegas.Project);
            //Load selected events from Track
            TrackEvent[] events = getSelectedEventsInTrack(track);
            

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

        
      
    }
}
