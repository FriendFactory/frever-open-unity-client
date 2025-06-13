namespace ARKit
{
    internal static class ARCoreFaceMeshConstants
    {
        //  values calculated from the ARCORE canonical_face_mesh.fbx all represent the player face perspective which is the mirror of the in game model
        public const int EYE_LEFT_INSIDE_CORNER = 362;  //FACEMESH_EyeLeftInsideCorner = 362;
        public const int EYE_LEFT_OUTSIDE_CORNER = 263; // FACEMESH_EyeLeftOutsideCorner = 263;
        public const int EYE_LEFT_TOP_LID = 386;        //FACEMESH_EyeLeftTopLid = 386;
        public const int EYE_LEFT_BOTTOM_LID = 374;     //FACEMESH_EyeLeftBottomLid = 374;
        public const int EYE_LEFT_TOP_OUTER_LID = 257;        
        public const int EYE_LEFT_BOTTOM_OUTER_LID = 253;     
        public const int EYE_RIGHT_INSIDE_CORNER = 133; //FACEMESH_EyeRightInsideCorner = 133;
        public const int EYE_RIGHT_OUTSIDE_CORNER = 33; //FACEMESH_EyeRightOutsideCorner = 33;
        public const int EYE_RIGHT_TOP_LID = 159;       //FACEMESH_EyeRightTopLid = 159;
        public const int EYE_RIGHT_BOTTOM_LID = 145;    //FACEMESH_EyeRightBottomLid = 145;
        public const int EYE_RIGHT_TOP_OUTER_LID = 27;        
        public const int EYE_RIGHT_BOTTOM_OUTER_LID = 23; 
        public const int BROW_UPPER_LEFT = 282; 
        public const int BROW_UPPER_RIGHT = 52;
        
        // public const int NOSE_TIP = 1;  /// maybe try 2 or ->164<- since they are on a closer plane vs 1 which is on a very different z point
        public const int MID_PHILTRUM = 164;
        //  const int NOSE_CENTER = 4;
        public const int CHIN_CENTER = 175;
        public const int LIP_LOW_CENTER_UPPER = 13;
        public const int LIP_HIGH_CENTER_LOWER = 14;
        public const int NOSTRIL_LEFT = 327; // 327 player left most lower nostril
        public const int NOSTRIL_RIGHT = 98; // 98 player right most lower nostril
        public const int CHEEK_LEFT = 330;   //  player Left 330 cheek,  I feel like it would be better with 253
        public const int CHEEK_RIGHT = 101;  //  player Right 101 cheek,  I feel like it would be better with 23
        public const int
            MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT = 287; // 287 player 2nd point outside left corner mouth
        public const int
            MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT = 57; // 57 player 2nd point outside right corner mouth
        public const int
            MOUTH_OUTSIDE_CORNER_1ST_POINT_LEFT = 291; // 291 player 1st point outside left corner mouth
        public const int
            MOUTH_OUTSIDE_CORNER_1ST_POINT_RIGHT = 61; // 61 player 1st point outside right corner mouth
        //public const int MOUTH_INSIDE_CORNER_LEFT = 308;
        //public const int MOUTH_INSIDE_CORNER_RIGHT = 78;
        public const int LIP_UPPER_OFF_CENTER_RIGHT = 72; // 72 upper lip right one vertex off center lip
        public const int LIP_LOWER_OFF_CENTER_RIGHT = 86; // 86 lower lip right one off center lip
        public const int LIP_UPPER_OFF_CENTER_LEFT = 302; // 302 upper lip Left one vertex off center lip
        public const int LIP_LOWER_OFF_CENTER_LEFT = 316; // 316 lower lip left one off center lip,
        // public const int LIP_LOWEST_PART = 17; //  17 is lowest part of the lower lip,
        public const int LIP_LOWER_MIDDLE_RIGHTHALF = 180; // 180 player right middle of right half of lower lip
        public const int
            CHIN_CENTER_TO_LIP_LOWEST_MID_POINT =
                200; // this point is the vector half way between the lip lowest and the Chin Center
    }
}
