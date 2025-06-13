package com.friendfactory.amplitude.experiment;

import android.app.Application;
import android.util.Log;

import com.amplitude.experiment.Experiment;
import com.amplitude.experiment.ExperimentClient;
import com.amplitude.experiment.ExperimentConfig;
import com.amplitude.experiment.Variant;

import java.util.*;

public class AmplitudeExperimentWrapper {
    private static final AmplitudeExperimentWrapper _instance = new AmplitudeExperimentWrapper();
    private static final String LogTag = "FREVER";

    public static AmplitudeExperimentWrapper getInstance() {return _instance; }

    private ExperimentClient _client;

    public AmplitudeExperimentWrapper() {
        Log.i(LogTag, "[Amplitude Experiment Wrapper] Wrapper has been created");
    }

    public void initialize(Application context, String key, String instanceName, ExperimentInitializedCallback callback)
    {
        Log.i(LogTag, "[Amplitude Experiment Wrapper] Initialize started");
    
        ExperimentConfig config = ExperimentConfig.builder().instanceName(instanceName).build();
        _client = Experiment.initializeWithAmplitudeAnalytics(context, key, config);
        
        try {
            _client.fetch(null).get();
            Log.i(LogTag, "[Amplitude Experiment Wrapper] Client has been initialized");
            callback.experimentInitialized(true);
        } catch (Exception e) {
            e.printStackTrace();
            callback.experimentInitialized(false);
        }
    }

    public String getVariantValue(String key)
    {
        if (_client == null) return "";

        Variant variant = _client.variant(key);

        return variant.value;
    }

    public String getPayloadValue(String key)
    {
        if (_client == null) return "";

        Variant variant = _client.variant(key);
        
        return variant.payload == null ? "" : String.valueOf(variant.payload);
    }
    
    public String[] getVariantsList()
    {
        if (_client == null) return null;

        Map<String, Variant> variants = _client.all();

        String[] result = new String[variants.size() * 2];

        int i = 0;

        for ( Map.Entry<String, Variant> entry : variants.entrySet() ) {
            String key = entry.getKey();
            Variant value = entry.getValue();
            result[i++] = key;
            result[i++] = value.value;
        }

        return result;
    }
}
