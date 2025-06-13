import Foundation
import AmplitudeExperiment

@objc public class AmplitudeExperiment : NSObject
{
    @objc public static let shared = AmplitudeExperiment()
    
    private var experiment : ExperimentClient?

    @objc public func Initialize(key : String, instanceName: String, callback: @escaping (Bool) -> Void)
    {
        NSLog("[Amplitude] Initializing Amplitude Experiment.")

        let config = ExperimentConfigBuilder()
            .instanceName(instanceName)
            .build()

        experiment = Experiment.initializeWithAmplitudeAnalytics(apiKey: key, config: config)

        experiment?.fetch(user: nil) {experiment, error in
            if (error != nil) {
                NSLog("[Amplitude] ExperimentSDK Error: %@", error.debugDescription)
                callback(false)
            } else {
                NSLog("[Amplitude] ExperimentSDK Initialized succesfully.")
                callback(true)
            }
        }
    }

    @objc public func GetVariantValue(key: String) -> String?
    {
        let variant = experiment?.variant(key)
        return variant?.value;
    }
    
    @objc public func GetPayloadValue(key: String) -> String?
    {
        let variant = experiment?.variant(key)
        let payload = variant?.payload as? String
        NSLog("[Amplitude] Payload: %@", payload ?? "")
        return payload;
    }
    
        
    @objc public func GetVariantsList() -> String?
    {
        let variants = experiment?.all()
        var result = [String]()

        if(variants == nil) {
            return nil;
        }

        variants?.forEach {
            result.append($0.0)
            result.append($0.1.value ?? "")
        }
        
        do{
            let data = try JSONSerialization.data(withJSONObject: result, options: JSONSerialization.WritingOptions.prettyPrinted)
            let json = String(decoding: data, as: UTF8.self)
            
            return json
        }
        catch {
            return nil
        }
    }
}