#!/usr/bin/env python
import argparse
import requests
import sys

local_url = "http://luxshare:bento@localhost:8100/api/1/"
rs_url = "http://luxshare:bento@10.90.104.16:8100/api/1/"  # SF results server IP. Most testings occur in SF
base_url = ""
# https://docs.google.com/document/d/1iiESDdm2Qhd8-rcwDn8C8FKgLZc09mBXnvwxsoJSRzU/edit

def check_connection():
    # will return "Connected" if the server is running
    url = base_url + "ping"
    response = requests.get(url)
    print(response.status_code)
    print(response.text)
    if "Connected" in response.text:
        return True


def post(results, endpoint="results", files=None):
    # will send the data to the server to be validated, but not saved
    # to save the values, this will need to be updated to endpoint="results"
    url = base_url + "firefly/" + endpoint
    data = {"run_results": results}
    response = requests.post(url, data=data, files=files)
    print("Result:%s"%response.status_code)
    print(response.text)


if __name__ == "__main__":
    parser = argparse.ArgumentParser("simulated firefly data api client")
    parser.add_argument("-x", "--local", action="store_true", help="run against a localhost api instance")      
    parser.add_argument("-s", "--station", action="append", help="station name")
    
    args = parser.parse_args()
    if args.local:
        print("local post:")
        base_url = local_url
    else:
        base_url = rs_url
    
    if not check_connection():
        sys.exit("Cannot connect to server")

    print("%s post:"%args.station[0])
    if args.station[0]=="MBLT":
        MBLT_json = open("./Data/Firefly_MBLT_example.json", "r").read()
        post(MBLT_json)
    elif args.station[0]=="MBFT":
        MBFT_json = open("./Data/Firefly_MBFT_example.json", "r").read()
        litepoint = open("./Data/litepoint.zip", "rb").read()
        post(MBFT_json, files={"mbft_litepoint.zip": litepoint})
    elif args.station[0]=="FRTT":
        FRTT_json = open("./Data/Firefly_FRTT_example.json", "r").read()
        post(FRTT_json)
    elif args.station[0]=="SFT":
        SFT_json = open("./Data/Firefly_SFT_example.json", "r").read()
        post(SFT_json)
    elif args.station[0]=="SRF":
        SRF_json = open("./Data/Firefly_SRF_example.json", "r").read()
        litepoint = open("./Data/litepoint.zip", "rb").read()
        post(SRF_json, files={"srf_litepoint.zip": litepoint})
    #elif args.station[0]=="RUNIN":
    #    RUNIN_json = open("./Data/Firefly_RUNIN_example.json", "r").read()
    #    post(RUNIN_json)
    elif args.station[0]=="RTT":
        RTT_json = open("./Data/Firefly_RTT_example.json", "r").read()
        post(RTT_json)
    elif args.station[0]=="CCT":
        CCT_json = open("./Data/Firefly_CCT_example.json", "r").read()
        post(CCT_json)
    #elif args.station[0]=="ORT":
    #    ORT_json = open("./Data/Firefly_ORT_example.json", "r").read()
    #    post(ORT_json)
    elif args.station[0]=="Repair":
        Repair_json = open("./Data/Firefly_Repair_example.json", "r").read()
        post(Repair_json)
    else: 
        print("Error!!station name %s is unkown!"%args.station[0])
