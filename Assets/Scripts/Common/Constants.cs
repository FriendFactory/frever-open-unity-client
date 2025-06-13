using System;
using UnityEngine;

namespace Common
{
    public static class Constants
    {
        public const string CONTACTS_INTRODUCTION_IDENTIFIER = "ContactIntroduction";
        public const string TERMS_OF_USE_LINK = "https://www.frever.com/eula";
        public const string PRIVACY_POLICY_LINK = "https://www.frever.com/privacy-policy";
        public const string OPEN_SOURCE = "https://www.frever.com/open-source";
        public const string DISCORD_LINK = "https://discord.gg/DJ46KVbNDu";
        public const string TIKTOK_LINK = "https://vm.tiktok.com/ZMRRHbu6m/";
        public const string INSTAGRAM_LINK = "https://instagram.com/freverofficial";
        public const string FAQ_LINK = "https://www.frever.com/faq/";
        public const string WEBPAGE_LINK = "https://www.frever.com";
        public const string FREVER_TESTER = "https://docs.google.com/forms/d/e/1FAIpQLScSYPYctqdqaQyiUK5zdhWQrc4dMR-0UsB8DatkBdiWWTSfMQ/viewform";
        public const string APP_STORE_URL = "https://apps.apple.com/app/frever/id1471858786";
        public const string EPIDEMIC_SUBSCRIPTION_URL = "https://www.epidemicsound.com/?utm_source=frever&utm_medium=partner&utm_campaign=&_us=partner&_usx=frever";
        public const string GOOGLE_SERVER_CLIENT_ID = "380589288197-2cvchatb1o8p7ia0cgjk6qmtcoijq1he.apps.googleusercontent.com";

        public const int MY_FREVERS_TAB_INDEX = 1;
        public const int STAR_CREATORS_TAB_INDEX = 2;
        public const int FRIENDS_TAB_INDEX = 3;

        public const int PERSISTENT_SCENE_INDEX = 1;

        public const int CHARACTERS_IN_SPAWN_FORMATION_MAX = CHARACTERS_IN_EVENT_MAX;
        public const int CHARACTERS_IN_EVENT_MAX = 3;
        public const int POPUPS_SORTING_LAYER = 100;
        public const int SNACKBAR_SORTING_LAYER = 200;
        
        public const float RESERVED_PROGRESS_BAR_FOR_VIDEO_CAPTURING = 0.5f;
        
        public static class FileDefaultNames
        {
            public const string VOICE_TRACK_FILE = "UserVoice.wav";
            public const string FACE_ANIMATION_FILE = "FaceAnimation.txt";
            public const string CAMERA_ANIMATION_FILE = "CameraAnimation.txt";
            public const string LEVEL_TEMP_FILE = "LevelTempSave.txt";
            public const string ANDROID_RENDERING_PROFILE = "Android Rendering Pipeline Profile";
        }
        
        public static class FileDefaultPaths
        {
            public const string SAVE_FOLDER = "Save";
            public static readonly string VOICE_TRACK_PATH = $"{SAVE_FOLDER}/{FileDefaultNames.VOICE_TRACK_FILE}";
            public static readonly string FACE_ANIMATION_PATH = $"{SAVE_FOLDER}/{FileDefaultNames.FACE_ANIMATION_FILE}";
            public static readonly string CAMERA_ANIMATIONS_FOLDER = $"{SAVE_FOLDER}/CameraAnimations";
            public static readonly string CAMERA_ANIMATION_PATH =  $"{CAMERA_ANIMATIONS_FOLDER}/{FileDefaultNames.CAMERA_ANIMATION_FILE}";
            public static readonly string LEVEL_TEMP_PATH = $"{SAVE_FOLDER}/{FileDefaultNames.LEVEL_TEMP_FILE}";
            public static readonly string RECORDED_VIDEOS_FOLDER = "Recordings";
            public static readonly string SENTRY_OPTIONS_PATH = "Assets/Resources/Sentry/SentryOptions.asset";
        }
        
        public static class LevelDefaults
        {
            public const int CAMERA_DOF_DEFAULT = 65000;
            public const float AUDIO_SOURCE_VOLUME_DEFAULT = 1;
            public const float BACKGROUND_MUSIC_VOLUME = 0.2f;//background relatively to the voice track
            public const float MAX_LEVEL_DURATION = 15f;
            public const float MAX_LEVEL_DURATION_STAR = 60f;
            public const int MIN_EVENT_DURATION_MS = 500;
        }
        
        public static class Memory
        {
            public const int MIN_DISK_SPACE_REQUIRED_MB = 700;
            public const int MIN_DISK_SPACE_REQUIRED_DISPLAY_MB = 1024;//display for user, a bit more than real required threshold
            
            public const int RESERVED_MEMORY_PER_SETLOCATION_MB_IOS = 150;
            public const int RESERVED_MEMORY_PER_SETLOCATION_MB_ANDROID = 150;
            public const int RESERVED_MEMORY_PER_CHARACTER_MB = 100;
            public const int RESERVED_MEMORY_FOR_UMA_BUILD_PROCESS_MB = 100;
            public const int RESERVED_MEMORY_BY_UNITY_MB = 100;//Unity has some supposed memory overhead due to mechanism under the hood(based on information from Unity Support)
        }
        
        public static class Character
        {
            public static readonly Bounds BOUNDS = new Bounds(Vector3.zero, Vector3.one * 2);//hardcoded size to cover almost all character animation without needed updating bounds every frame
        }

        public static class Onboarding
        {
            public const string SEEN_SEASON_POPUP_IDENTIFIER = "SeenSeasonPopup";
            public const string RECEIVED_START_GIFT_IDENTIFIER = "ReceivedStartGift";
            public const string CONTEXT_LOCAL_FILE_PATH = "Onboarding/Context";
            public const string HIGHLIGHT_COLOR_STRING = "F95CAC";
            public const int MIN_PASSWORD_LENGTH = 8;
        }

        public static class CharacterEditor
        {
            public const string START_BODY_ANIM_NAME = "Idle";
        }
        
        public static class Wardrobes
        {
            public const string HAIR_CATEGORY_NAME = "Hair";
            public const string FACIAL_HAIR_CATEGORY_NAME = "Facial Hair";
            public const string EYE_BROWS_CATEGORY_NAME = "Eyebrows";

            public const long OUTFIT_CATEGORY_ID = 10;
            public const long DRESS_UP_CATEGORY_ID = -99999999;
            public const long FAVOURITE_OUTFIT_SUBCATEGORY_ID = -99999999;
            public const long RECENT_OUTFIT_SUBCATEGORY_ID = -88888888;
            public const long COLLECTIONS_CATEGORY_ID = -77777777;
            public const long BODY_CATEGORY_TYPE_ID = 1;
            public const long CLOTHES_CATEGORY_TYPE_ID = 2;
            public const long MAKEUP_CATEGORY_TYPE_ID = 3;
            public const long DRESS_UP_CATEGORY_TYPE_ID = 4;
        }
        
        public static class Genres
        {
            public const long NEW_GENRE_ID = 29;
            public const string NEW_GENRE_NAME = "New";
            public const int NEW_GENRE_SORT_ORDER = 5;
        }
        
        public static class CreatorScore
        {
            public static readonly Color[] BadgeColors =
                {
                    Color.white,
                    Color.white,
                    Color.white,
                    Color.white,
                    new Color32(107, 177, 250, 255),
                    new Color32(181, 137, 255, 255), 
                    new Color32(198, 68, 134, 255)
                };

            public const int DISPLAY_BADGE_FROM_LEVEL = 1;
            public const int DISPLAY_COMMENT_BADGE_FROM_LEVEL = 4;
        }

        public static class ErrorMessage
        {
            public const string BROKEN_CHARACTER = "Something went wrong with the character you are using.\n Please try a different one";
            public const string WRONG_FILE_FORMAT = "You uploaded a file format we don’t support. Please try a different format";
            public const string COPYRIGHT_ERROR_IDENTIFIER = "CopyrightedContent";
            public const string MODERATION_FAILED_ERROR_CODE = "Video content is inappropriate";
            public const string TEMPLATE_MODERATION_FAILED_ERROR_CODE = "TemplateNameModerationNotPassed";
            public const string ASSET_INACCESSIBLE_IDENTIFIER = "Inaccessible";
            public const string SOUND_INACCESSIBLE_IDENTIFIER = "SongNotAvailable";
            public const string UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_ASSETS = "Cannot save because assets are no longer accessible";
            public const string UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_SOUND = "Cannot save because used soundtrack is no longer accessible";
        }

        public static class Gestures
        {
            public const string ALLOW_GESTURE_PASSTHROUGH_TAG = "PassThrough";
        }
        
        public static class AppleId
        {
            public const string USER_ID_IDENTIFIER = "AppleUserId";
        }

        public static class SourceFileSizes
        {
            public static readonly Vector2 CONVERT_VIDEO_SOURCE_FILE_SIZE_RANGE_MB = new Vector2(1f, 25f);
            public static readonly Vector2 CONVERT_IMAGE_SOURCE_FILE_SIZE_RANGE_MB = new Vector2(0.015f, 7f);
            public static readonly Vector2 CONVERT_AUDIO_SOURCE_FILE_SIZE_RANGE_MB = new Vector2(0.3f, 25f);
        }
        
        public static class NavigationMessages
        {
            public const string DEFAULT_LEVEL_EDITOR_MESSAGE = "Welcome to Frever Editor!";
        }

        public static class LoadingPopupMessages
        {
            public const string SAVING_PROGRESS_MESSAGE = "Saving...";
            public const string LOADING_PROGRESS_MESSAGE = "Loading...";
            
            public const string LEVEL_EDITOR_HEADER = "Opening Studio editor";
            public const string TASK_HEADER = "Submitting video... Please do not close the app";
            public const string WARDROBE_HEADER = "Opening Wardrobe";
            public const string VIDEO_MESSAGE_HEADER = "Opening Moments editor";
            public const string VIDEO_PUBLISHING_HEADER = "Publishing video... Please do not close the app";
        }

        /// <summary>
        /// Used for loading files from backend by keys
        /// </summary>
        public static class FileKeys
        {
            public const string LEVEL_CREATION_LOCKED_BG = "popup/level_creation/locked";
            public const string LEVEL_CREATION_UNLOCKED_BG = "popup/level_creation/unlocked";
            public const string CREATE_FREVER_CHARACTER_POPUP = "popup/character/force_creation";
        }

        public static class TasksPageTabs
        {
            public const int STYLE_BATTLES = 0;
            public const int IN_VOTING = 1;
        }

        public static class DotIndicators
        {
            public const string Invite = "InviteDotIndicators";
        }
        
        public static class Templates
        {
            public const int TEMPLATE_PAGE_SIZE = 18;
        }
        
        public static class Features
        {
            public static readonly bool AI_REMIX_ENABLED = false;//todo: it's temporary disabling the feature. Please drop when we release it
            public static readonly string VIDEO_TO_FEED_UNLOCKED_POPUP_DISPLAYED = "VIDEO_TO_FEED_UNLOCKED_POPUP_DISPLAYED";
        }

        public static class ProfileLinks
        {
            public const string LINK_KEY_TIKTOK = "tiktok_link";
            public const string LINK_KEY_INSTAGRAM = "instagram_link";
            public const string LINK_KEY_YOUTUBE = "youtube_link";
        }

        public static class Quests
        {
            public const long LIKE_VIDEOS_QUEST_ID = 2;
            public const long SEASON_QUEST_GROUP_NUMBER = 3;
        }
        
        public static class LicensedMusic
        {
            public static class Constraints
            {
                public const int LICENSED_SONGS_IN_LEVEL_COUNT_MAX = 4;
                public const float LICENSED_SONG_DURATION_USAGE_MAX_SEC = 15;
                public const float LICENSED_SONGS_CLIP_LENGTH_SEC = 30;
            }

            public static class Messages
            {
                public static readonly string UNIQUE_MUSIC_PER_LEVEL_LIMIT_REACHED = $"Sorry you are only allowed to use {Constraints.LICENSED_SONGS_IN_LEVEL_COUNT_MAX.ToString()} unique songs per video";
                public static readonly string MAX_DURATION_PER_LEVEL_LIMIT_REACHED = $"Sorry, you are only allowed to record up to {Constraints.LICENSED_SONG_DURATION_USAGE_MAX_SEC}s per song";
            }
        }

        public static class Crew
        {
            public const string LAST_ANNOUNCEMENT_CLOSED_WEEK = "last_announcement_closed_week";
            public const int LEADER_ROLE_ID = 1;
            public const int COORDINATOR_ROLE_ID = 2;
            public const int ELDER_ROLE_ID = 3;
            public const int MEMBER_ROLE_ID = 4;
            public const int RECRUIT_ROLE_ID = 5;

            public const string JOIN_REQUEST_HANDLED_KEY = "CrewRequestApproved_{0}";
        }
        
        public static class Feedback
        {
            public const string FEEDBACK_EMAIL = @"mailto:support@frever.com?subject=Frever%20app%20feedback";
        }

        public static class VideoMessage
        {
            public const float SCALE_MIN = 0.3f;
            public const float SCALE_MAX = 3f;
            public const int BACKGROUNDS_PAGE_SIZE = 20;
            public const int BACKGROUNDS_CACHE_CAPACITY = 5;
        }

        public static class VideoRenderingResolution
        {
            public static readonly Vector2Int PORTRAIT_720 = new Vector2Int(720, 1280);
            public static readonly Vector2Int PORTRAIT_1080 = new Vector2Int(1080, 1920);
            public static readonly float PORTRAIT_ASPECT = PORTRAIT_1080.x / (float) PORTRAIT_1080.y;
            public static readonly Vector2Int LANDSCAPE_1080 = new Vector2Int(1920, 1080);
            public static readonly Vector2Int LANDSCAPE_2160 = new Vector2Int(3840, 2160);
            public static readonly Vector2Int LANDSCAPE_720 = new Vector2Int(1280, 720);
        }

        public static class ThumbnailSettings
        {
            public const float CHARACTER_PHOTO_BOOTH_RENDER_SCALE = 2f;
            public const float PROFILE_PHOTO_BOOTH_RENDER_SCALE = 2f;
        }
        
        public static class Captions
        {
            public const int DEFAULT_FONT_SIZE = 80;
            public const float INT_POS_MULTIPLIER = 10000f;
            public const float NORMALIZED_POS_MIN = 0f;
            public const int POS_MIN = (int)(NORMALIZED_POS_MIN * INT_POS_MULTIPLIER);
            public const float NORMALIZED_POS_MAX = 1f;
            public const int POS_MAX = (int)(NORMALIZED_POS_MAX * INT_POS_MULTIPLIER);
            public const int CAPTIONS_PER_EVENT_MAX = 16;
            public const string REACHED_LIMIT_MESSAGE = "Maximum Caption Limit Reached, remove existing captions to add new ones.";
            public const float FONT_SIZE_MIN = 30;
            public const float FONT_SIZE_MAX = 160;
        }
        
        public static class Feed
        {
            public const ScaleMode DEFAULT_SCALE_MODE = ScaleMode.ScaleAndCrop;
            public const int LIKES_GIVEN_TO_RECOMMEND_FOLLOWING = 3;
        }

        public static class HomePage
        {
            public const string ANDROID_BT_HEADSET_WARNING_DISPLAYED = "AndroidBtHeadsetWarningPopupDisplayed";
        }

        public static class Regexes
        {
            public const string VIDEO_LINK = @"^https://web\.frever-api\.com/video/([a-z])_([a-zA-Z0-9]+)$";
        }
        
        public static class Binding
        {
            public const string WATERMARK_ANIMATION = "Watermark Animation";
        }
        
        public static class RaceIds
        {
            public const long FREVER = 1;
            public const long SIMS = 2;
        }
        
        public static class Notifications
        {
            public static readonly TimeSpan GROUP_NOTIFICATIONS_THRESHOLD = TimeSpan.FromMinutes(10);
            public const int GROUP_SIZE_MIN = 3;
        }
    }
}