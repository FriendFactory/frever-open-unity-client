#!/bin/bash

# Function to modify a specific line in a file
modify_line() {
    local file="$1"
    local line_number="$2"
    local old_text="$3"
    local new_text="$4"

    # Check if the file exists
    if [ -f "$file" ]; then
        # Replace the specified text in the specified line
        sed -i '' "${line_number}s/${old_text}/${new_text}/" "$file"
        echo "Modified line ${line_number} in file ${file}"
    else
        echo "File ${file} not found"
    fi
}

# Check if path argument is provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <path>"
    exit 1
fi

# Path to the Xcode project directory
path="$1"

# Modify GDTCORClock.m
filePath="${path}/Pods/GoogleDataTransport/GoogleDataTransport/GDTCORLibrary/GDTCORClock.m"
modify_line "$filePath" 44 "KernelBootTimeInNanoseconds()" "KernelBootTimeInNanoseconds(void)"
modify_line "$filePath" 62 "UptimeInNanoseconds()" "UptimeInNanoseconds(void)"

# Modify GDTCORPlatform.m
filePath="${path}/Pods/GoogleDataTransport/GoogleDataTransport/GDTCORLibrary/GDTCORPlatform.m"
modify_line "$filePath" 87 "GDTCORNetworkTypeMessage()" "GDTCORNetworkTypeMessage(void)"
modify_line "$filePath" 102 "GDTCORNetworkMobileSubTypeMessage()" "GDTCORNetworkMobileSubTypeMessage(void)"
modify_line "$filePath" 157 "GDTCORDeviceModel()" "GDTCORDeviceModel(void)"

# Modify GDTCCTNanopbHelpers.m
filePath="${path}/Pods/GoogleDataTransport/GoogleDataTransport/GDTCCTLibrary/GDTCCTNanopbHelpers.m"
modify_line "$filePath" 166 "GDTCCTConstructClientInfo()" "GDTCCTConstructClientInfo(void)"
modify_line "$filePath" 179 "GDTCCTConstructiOSClientInfo()" "GDTCCTConstructiOSClientInfo(void)"
modify_line "$filePath" 205 "GDTCCTConstructNetworkConnectionInfoData()" "GDTCCTConstructNetworkConnectionInfoData(void)"
modify_line "$filePath" 226 "GDTCCTNetworkConnectionInfoNetworkMobileSubtype()" "GDTCCTNetworkConnectionInfoNetworkMobileSubtype(void)"

# Modify AppsFlyer+AppController.m
filePath="${path}/Libraries/appsflyer-unity-plugin/Plugins/iOS/AppsFlyer+AppController.m"
modify_line "$filePath" 144 ", (UIBackgroundFetchResult)" ", int(UIBackgroundFetchResult)"
