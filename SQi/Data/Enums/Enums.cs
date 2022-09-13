﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Enums
{
    public enum MedicalTestStatus : int
    {
        UNFINISHED = 1,
        FINISHED = 2,
        CANCELED = 3,
        NOT_DOING = 4,
        DOCTOR_CANCEL = 5,
    }

    public enum BookingStatus : int
    {
        UNFINISHED = 1,
        FINISHED = 2,
        CANCELED = 3,
        NOT_DOING = 4,
        DOCTOR_CANCEL = 5,
        RESULTED = 6
    }

    public enum TestingType : int
    {
        LAY_TEST  =1,
        VIRAL_LOAD = 2,
        CD4 = 3,
        RECENCY = 4,
        HTS_POS = 5
    }

    public enum ResultTesting : int
    {
        POSITIVE = 1,
        NEGATIVE = -1,
        NO_RESULT = 0
    }

    public enum TypeHTS_POS
    {
        PREP =1,
        ARV =2
    }

    public enum SesstionType : int
    {
        CONSULSTATION = 0,
        LAY_TEST = 1,
        RECENCY = 2,
        PrEP = 3,
        ART = 4
    }

    public enum ReferType:int
    {
        TESTING,
        PrEP,
        ART
    }


}
