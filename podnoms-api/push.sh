web-push send-notification \
    --endpoint="https://fcm.googleapis.com/fcm/send/dAa2ejxsN1E:APA91bH522yYqXlaSSsqGmyELdpf9K3OhbHzakvyhXEKI8fAEEQkrVQSv28fg-VVSyvD-8NqgiLGXm-UI7HcwY-lzsHvYi8hKeu_vbpQ9Xdh7wQP_jTiUDmxwK7TFHeK7_BvfsgXJcssBZb3-McHznazmoXkKVhLcg" \
    --key="BDCl0Kw2MUckep1GdF9WJhZ1WT7gAe456rmB-nT95N_0925hbCdUmdUGk71eqzf1T_Wf2ohYAPXzVDGM62S4Wfg" \
    --auth="C0EMaBqxIoxICwvyq9NnHA" \
    --payload='{"test": "Hallo Sailor", "notification": {"title": "Hello Sailor", "body": "The greatest dancer"}}' \
    --vapid-subject="http://localhost:8000" \
    --vapid-pubkey="BJQY5jNSGoa3SVqxlHH3fyhpBx_7pMrqijh92bM4cwZlmfSYrsRG-8Ci1VYkHr3W13Uh2nWmLTRL00pc7HBdias" \
    --vapid-pvtkey="eFxm_sq3OesJ1ZZUCWCJ0uGqpeWXimOunTqt4CSfwjw"
