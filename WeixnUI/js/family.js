wx.ready(function () {
 
function chooseUpload(selector) {
    
    wx.chooseImage({
        success: function (res) {
            $("#loading").show();
            $(function () {
                $.each(res.localIds, function (i, n) {

                    wx.uploadImage({
                        localId: res.localIds.toString(), // 需要上传的图片的本地ID，由chooseImage接口获得
                        isShowProgressTips: 0, // 默认为1，显示进度提示
                        success: function (res1) {
                            $.ajax({
                                url: 'http://www.focuswin.cn/wxapi/face/detect/' + res1.serverId,
                                dataType: "json",
                                success: function (data) {
                                    
                                    var arr = JSON.parse(data),
                                        ageArr = [arr[0].age, arr[1].age, arr[2].age],
                                        childFaceId, fatherFaceId, motherFaceId;
                                    if (JSON.parse(data).length == 3 && !(arr[0].age == arr[1].age == arr[2].age) && !(arr[0].gender == arr[1].gender == arr[2].gender)) {
                                        
                                        var minAge = Math.min.apply(Math, ageArr);
                                        for (var i = 0; i < ageArr.length; i++) {
                                            if (minAge == ageArr[i]) {
                                                childFaceId = arr[i].faceId;
                                                arr.splice(i, 1);
                                                for (var j = 0; j < arr.length; j++) {
                                                    if (arr[j].gender == 'male') {
                                                        fatherFaceId = arr[j].faceId;
                                                        arr.splice(j, 1);
                                                    }
                                                }
                                            }
                                        }
                                        motherFaceId = arr[0].faceId;
                                        $(selector).data('childFaceId', childFaceId)
                                                   .data('fatherFaceId', fatherFaceId)
                                                   .data('motherFaceId', motherFaceId)
                                        $(selector).html('<img src="' + n + '" /> <br />');
                                        $("#loading").hide();
                                    } else if (JSON.parse(data).length == 0) {
                                        alert('啊，我看不到你的脸~')
                                    } else {
                                        alert('请选择一家三口的照片')
                                    }

                                }
                            })

                        },
                        fail: function (res) {
                            alert(JSON.stringify(res));
                        }
                    });
                });
            });
        }
    });
}
document.querySelector('#familyPic').onclick = function () {
    chooseUpload('#familyPic')
      
  };



  function verify(selector,like,parent, child) {
      $("#loading").show();
      $.ajax({
          url: 'http://www.focuswin.cn/wxapi/face/verify/' + parent + '/' + child,
          dataType: "json",
          success: function (data) {
              $("#loading").hide();
              $(selector).html(like + (JSON.parse(data).confidence * 100).toFixed(2) + '%')
          }
      })
      
  }
  document.querySelector('#uploadImage').onclick = function () {
      var parent1Img = $('#familyPic').data('fatherFaceId');
      var parent2Img = $('#familyPic').data('motherFaceId');
      var childImg = $('#familyPic').data('childFaceId');
      $("#loading").show();
      verify('.fanilylike1','与爸爸的相似度为：', parent1Img, childImg);
      verify('.fanilylike2','与妈妈的相似度为：', parent2Img, childImg);
      
     
  }

  var shareData = {
      title: '测测孩子跟谁像',
      desc: '来看看孩子跟爸爸比较像还是跟妈妈比较像',
      link: window.location.href,
      imgUrl: 'http://www.focuswin.cn/WeFace/fonts/family.jpg'
  };
  wx.onMenuShareAppMessage(shareData);
  wx.onMenuShareTimeline(shareData);

  function decryptCode(code, callback) {
    $.getJSON('/jssdk/decrypt_code.php?code=' + encodeURI(code), function (res) {
      if (res.errcode == 0) {
        codes.push(res.code);
      }
    });
  }
});

wx.error(function (res) {
  alert(res.errMsg);
});

