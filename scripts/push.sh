web-push send-notification \
    --endpoint="https://fcm.googleapis.com/fcm/send/fITVQi0gAmw:APA91bG1F3zq48GikCX2r2NX0wZgbpK-yngw4pVMTOKWJRIJ4dIL9csR7q3v6Kseixbcygte2ommldJDjCAsJVHeViARtraM2W_57bjFe37eeID72V4ObVE10fkZwsK_c_CpuCf0wBLJ" \
    --key="BA6PXDQIgLBbQjcRzQOK4D7q8Sjh69hTB27IUcKdkKfTYE0gqdz7ZnqhWJCMJMBNgWEWR21YZp5xIxh35f0ys-o" \
    --auth="QoJMeA3d7_3E6czncW-B7A" \
    --payload='{"test": "Hallo Sailor", "notification": {"title": "Hello Sailor", "body": "The greatest dancer"}}' \
    --vapid-subject="http://localhost:8000" \
    --vapid-pubkey="BNSiPlkIlKGspSOAHG33wgCChPkKbGJDg3ov3vd_LsGXEtPFQvWOrbouH1dZ2kt-ahQzSgUX2BCN_kEbzPf6Yxc" \
    --vapid-pvtkey="jXtLMUzIavtqlHyJRbDt88bWgKQ5NZoUIeJfk7-Ee6k"
