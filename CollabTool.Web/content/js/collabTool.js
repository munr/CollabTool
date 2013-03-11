(function(){
	var students = [],
		allNotes = [],
        selectedStudentId,
        position,
		sidebarDiv = $("#sidebar"),
		studentSearchBox = $("#txtStudentSearchBox"),
		studentListItemClass = ".studentListItem",
		studentListItemHtml = $("#studentListItemTemplate").html(),
		studentListItemTemplate = Handlebars.compile(studentListItemHtml),
		studentListElement = $("#studentList"),

		contentElement = $("#content"),
		mainContentElement = $("#mainContent"),
		newNoteFormElement = $("#newNoteForm"),
		studentDetailsHtml = $("#studentDetailsTemplate").html(),
		studentDetailsTemplate = Handlebars.compile(studentDetailsHtml),
		studentDetailsContainerDiv = $("#studentDetailsContainer"),
		studentDetailsDiv = $("#studentDetails"),
		studentMoreOrLessBtn = $("#moreOrLessBtn"),
		studentNoteHtml = $("#studentNoteTemplate").html(),
		studentNoteTemplate = Handlebars.compile(studentNoteHtml),
		studentNotesContainerDiv = $("#studentNotesContainer"),
		studentNotesDiv = $("#studentNotes"),
		removeNoteBtnClass = ".removeNoteBtn",
		buttonsBarDiv = $("#buttonsBar"),
		addNoteBtn = $("#addNoteBtn"),
		closeNewNoteFormBtn = $("#closeNewNoteFormBtn"),
		newNoteDiv = $("#newNoteDiv"),
		saveNewNoteBtn = $("#saveNewNoteBtn"),
		cancelNewNoteBtn = $("#cancelNewNoteBtn"),

		studentDisciplineObjectHtml = $("#studentDisciplineObject").html(),
		studentDisciplineObjectTemplate = Handlebars.compile(studentDisciplineObjectHtml),
		notesSearchBox = $("#txtNotesSearchBox"),
		noteRankFilterIconClass = $('.noteRankFilterIcon'),
		newNoteRankDropdown = $("#newNoteRankDropdown");

	function init() {
		// TODO replace this with an AJAX call
		var mockStudents = [
			{ 'studentId' : '1', 'studentName' : 'Mike', 'studentIconUrl' : '/content/img/student-icon-male.png',				
				testScores: [
	            {
	                name: 'Class Average',
	                data: [[new Date('2013-01-13').getTime(), 76], [new Date('2013-3-14').getTime(), 72], [new Date('2013-5-2').getTime(), 81], [new Date('2013-5-14').getTime(), 82], [new Date('2013-6-17').getTime(), 73]]
	            }, {
	                name: 'Bob', // replace with student name
	                data: [[new Date('2013-01-07').getTime(), 72], [new Date('2013-02-02').getTime(), 76], [new Date('2013-03-14').getTime(), 84], [new Date('2013-5-18').getTime(), 87], [new Date('2013-6-21').getTime(), 73]]
	            }],     
				attendanceData: [
                    ['In Attendance',   45],
                    ['Tardy',       6],                   
                    ['Miss',    8]
                ],
                notes : [
							{ 	
								noteTitle: 'Message 1', 
								noteText: 'Bacon ipsum dolor sit amet andouille tongue turducken, pastrami ham ball tip filet mignon.', 
								userIconUrl: '/content/img/student-icon-male.png',
								rank:'discipline',
								behaviorType: 'State Offense',
								didSendEmail: true,
								emailRecipients: 'mikeng13@gmail.com, daomannhi@gmail.com',
								resolved: true,
								location: 'School Cafeteria',
								noteTimestamp: '12:43pm',
								incidentTime: 'Jan 12, 2013',
								userName: 'Michael Ng',
								isNote: false
							},
							{ 
								noteTitle: 'Message 2', 
								noteText: 'Ipsum dolor sit amet andouille tongue turducken, pastrami ham ball tip filet mignon. Venison jowl shankle corned beef short ribs t-bone fatback tongue, drumstick pig.',
								userIconUrl: '/content/img/student-icon-female.png',	
								rank:'comment',
								didSendEmail: true,
								emailRecipients: 'daomannhi@gmail.com, mikeng13@gmail.com',
								userName: 'Nhi Dao',
								noteTimestamp: '12:43pm',
								userName: 'Nhi Dao',
								isNote: true
							},
							{ 	
								noteTitle: 'Message 3', 
								noteText: 'Bacon ipsum dolor sit amet andouille tongue turducken, pastrami ham ball tip filet mignon. Venison jowl shankle corned beef short ribs t-bone fatback tongue, drumstick pig.', 
								userIconUrl: '/content/img/student-icon-female.png',
								rank:'discipline',
								resolved: false,
								didSendEmail: false,
								behaviorType: 'School Violation',
								location: 'School Bus',
								noteTimestamp: '12:43pm',
								incidentTime: 'Feb 28, 2013',
								userName: 'Nhi Dao',
								isNote: false
							},
						]
			 },
			{ 'studentId' : '2', 'studentName' : 'Nhi', 'studentIconUrl' : '/content/img/student-icon-female.png' }
		];

		$.ajax({
		    type: 'GET',
            url: '/Api/GetStudents',
            success: function (students) {
                getStudentsCallback(students);
            }
		});

		// set up the click handlers
		studentListElement.delegate(studentListItemClass, 'click', studentListItemClickHandler);
		studentSearchBox.keyup(studentSearchBoxChangeHandler);
		studentNotesDiv.delegate(removeNoteBtnClass, 'click', removeNoteBtnClickHandler);
		studentNotesDiv.delegate('.noteRedFlag', 'click', noteRedFlagClickHandler);
		addNoteBtn.click(addNoteClickHandler);
		closeNewNoteFormBtn.click(closeNewNoteFormClickHandler);
		saveNewNoteBtn.click(saveNewNoteClickHandler);
		notesSearchBox.keyup(notesSearchBoxChangeHandler);
		noteRankFilterIconClass.click(noteRankFilterClickHandler);
		newNoteRankDropdown.change(newNoteRankDropdownChangeHandler);

		$("#newNoteDateTime").datepicker();
		$("#noteDateResolved").datepicker();

		$('.apply-select2').select2({ width: 'element'});
	}

	/**
	 *
	 */
	function getStudentsCallback(studentList) {
	    // cache the student list
	    students = studentList;
		_.each(studentList, function(student) {		
			// insert into DOM
			addStudentToList(student);
		});
	}

	/**
	 *
	 */
	function studentSearchBoxChangeHandler(event) {
		var input = $(event.currentTarget),
			value = input.val();
		filterStudentList('Name', value);
	}

	/**
	 *
	 */
	function filterStudentList(attribute, val) {
		studentListElement.empty();
		_.each(students, function(student) {
			var attrVal = student[attribute];
			if (attrVal.indexOf(val) !== -1) {
			 	addStudentToList(student);
			}
		});
	}

	/**
	 *
	 */
	function addStudentToList(student) {
		// check if the student has any incidents
		//var 

		studentListElement.append(studentListItemTemplate(student));
	}

	/**
	 *
	 */
	function studentListItemClickHandler(event) {
		var studentListItem = $(event.currentTarget),
		studentId = studentListItem.data('student-id');
		
	    // keep track of selected student
		selectedStudentId = studentId;

		// TODO make AJAX call to get the student's details
		$.ajax({
		    type: 'GET',
		    url: '/Api/GetStudentDetail?studentId=' + studentId,
		    success: function (student) {
		        displayStudentDetails(student);
		    }
		});
		
		$.ajax({
		    type: 'GET',
		    url: '/Api/GetNotes?studentId=' + studentId,
		    success: function (result) {
		        var notes = result.Notes;
		        allNotes = notes;
		        displayStudentNotes(notes);

                // discipline charts uses the notes
		        drawDisciplineChart(notes);
		    }
		});
	}

	/**
	 *
	 */
	function getStudentDetailsCallback(student) {
		// display the student on screen
		displayStudentDetails(student);

		// cache the student details in memory
		var s = students[student.studentId];
		if (s) {
			s.details = student;
		}
	}

	/**
	 *
	 */
	 function displayStudentDetails(student) {
	 	// insert student into the DOM
	 	var studentDetailHtml = studentDetailsTemplate(student);
	 	studentDetailsDiv.html(studentDetailHtml);

	 	// TODO prepare data for charts
	 	drawPerformanceChart(student);

	     // make AJAX call to get attendance data
	 	$.ajax({
	 	    type: 'GET',
	 	    url: '/Api/GetAttendanceCount?studentId=' + selectedStudentId,
	 	    success: function (attendances) {
	 	        var attendanceData = [];
	 	        for (var i = 0; i < attendances.length; i++) {
	 	            var attendance = attendances[i];
	 	            attendanceData.push([attendance.name, attendance.value]);
	 	        }
	 	        drawAttendanceChart(attendanceData);
	 	    }
	 	});
	 }

	 /**
	  *
	  */
	 function drawAttendanceChart(attendanceData) {
	 	var chart = new Highcharts.Chart({
            chart: {
                renderTo: 'attendanceChart',
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                backgroundColor: 'transparent'
            },
            title: {
                text: 'Attendance',
                style: {
                	color: 'black'
                }
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        color: '#000000',
                        connectorColor: '#000000',
                        formatter: function() {
                            return '<b>'+ this.point.name +'</b>: '+ this.y;
                        }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: 'Attendance',
                data: attendanceData
            }]
        });    
	}

	 /**
	  *
	  */
	 function drawPerformanceChart(student) {
         // MOCK DATA
	     var performanceData = [
              {
                  name: 'Class Average',
                  data: [[new Date('2013-01-13').getTime(), 76], [new Date('2013-3-14').getTime(), 72], [new Date('2013-5-2').getTime(), 81], [new Date('2013-5-14').getTime(), 82], [new Date('2013-6-17').getTime(), 73]]
              }, {
                  name: student.Name, // replace with student name
                  data: [[new Date('2013-01-07').getTime(), 72], [new Date('2013-02-02').getTime(), 76], [new Date('2013-03-14').getTime(), 84], [new Date('2013-5-18').getTime(), 87], [new Date('2013-6-21').getTime(), 73]]
              }];
	     //var performanceData = student.testScores,
	 		chart = new Highcharts.Chart({
	 		chart: {
                renderTo: 'testScoresChart',
                backgroundColor: 'transparent',
                type: 'line',
                marginRight: 130,
                marginBottom: 25
            },
            title: {
                text: 'Test scores over time',
                x: -20, //center,
                style: {
                	color: 'black'
                }
            },
            labels: {
            	style: {
            		color: 'black'
            	}
            },	
            xAxis: {                
                labels: {
                	style: {
                		color: 'black'
                	}
                },
                type: 'datetime',
                dateTimeLabelFormats: { // don't display the dummy year
                    month: '%e. %b',
                    year: '%b'
                }
            },
            yAxis: {
                title: {
                    text: 'Test scores (%)',
                    style: {
                    	color: 'black'
                    }
                },
                labels: {
                	style: {
                		color: 'black'
                	}
                },
                plotLines: [{
                    value: 0,
                    width: 1,
                    color: '#808080'
                }]
            },
            legend: {
                layout: 'vertical',
                align: 'right',
                verticalAlign: 'top',
                x: -10,
                y: 100,
                borderWidth: 0,
                itemStyle: {
                	color: 'black'
                }
            },
            series: performanceData
	 	});
	 }

	 function drawDisciplineChart(notes) {	 	
	 	var disciplineItems = _.filter(notes, function(note) {
	 		return note.NoteType === 'discipline';
	 	});
        
	 	disciplineItems = _.sortBy(disciplineItems, function (item) {
	 	    return new Date(item.IncidentTime).getTime();
	 	});

	 	$("#disciplineChart").empty();
	 	_.each(disciplineItems, function(note){

	 		//assign color based on behavior type
	 		var resolved = note.Resolved,
	 			color;
	 		if (resolved) {
	 			color = 'green';
	 		} else {
	 			color = 'red';
	 		}

	 		note.color = color;
	 		var noteHtml = studentDisciplineObjectTemplate(note);
	 		var template = Handlebars.compile($("#studentDisciplineObjectTooltip").html());	 		
	 		$("#disciplineChart").append(noteHtml);
	 		$("#disciplineChart").find('.studentDisciplineObject').last().popover({
	 			placement:'top',
	 			trigger: 'hover',
	 			html: true,
	 			title: template(note)
	 		});
	 	})
	 }

	 /**
	  *
	  */
	 function getNotesCallback(notes) {
	 	displayStudentNotes(notes);
	 }

	 /**
	  *
	  */
	 function notesSearchBoxChangeHandler(event) {
	 	var input = $(event.currentTarget),
			value = input.val();
		filterNotes(['Subject', 'Text'], value);
	 }

	 /**
	  *
	  */
	 function noteRankFilterClickHandler(event) {
	 	var currentSelectedRankElem = $('.noteRankFilterIcon.rankSelected'),
	 		currentSelectedRank = currentSelectedRankElem.data('rank');	 		

	 	noteRankFilterIconClass.removeClass('rankSelected');
	 	var rankElem = $(event.currentTarget);
	 	var rankValue = rankElem.data('rank');

	 	if (rankValue === currentSelectedRank) {
	 		filterNotes(['NoteType'], ''); //reset filter
	 	} else if (rankValue === 'mail') {
	 	    filterNotes(['EmailNotificationSent'], true);
	 	}
	 	else {
	 	    rankElem.addClass('rankSelected');
	 	    filterNotes(['NoteType'], rankValue);
	 	}

	 }

	 /**
	  *
	  */
	 function filterNotes(attributes, value) {
	 	var filteredNotes = [];
	 	_.each(allNotes, function(note) {
	 		var numAttributes = attributes.length;
	 		for (var i = 0; i < numAttributes; i++) {
	 			var attribute = attributes[i];
	 			var attrVal = note[attribute];
	 		    try{
	 		        if (attrVal === value || attrVal.indexOf(value) !== -1) {
	 		            filteredNotes.push(note);
	 		            break;
	 		        }
	 		    }
	 		    catch (ex) {
	 		    }
	 		}
	 	});

	 	displayStudentNotes(filteredNotes);
	 }

	 /**
	  *
	  */
	 function displayStudentNotes(notes) {
	    position = 'Left';

	 	studentNotesDiv.empty();
	  	// insert note into the DOM
	  	_.each(notes, function(note) {
	  	    note.notePosition = position;
	  	    var dateTime = note.DateTime;
	  	    try {
	  	        note.DateTime = new Date(parseInt(note.DateTime.replace("/Date(", "").replace(")/", ""), 10)).toDateString();
	  	    }
	  	    catch (ex) {
	  	    }                           

	  		note.left = position === 'Left';
	  		note.right = position === 'Right';

	  		switch(note.NoteType) {
	  			case 'discipline' : 
	  			    note.rankUrl = '/content/img/important.png';
	  			    note.ShowFlag = true;                    
	  				break;
	  			case 'compliment' :
	  				note.rankUrl = '/content/img/arrow_up.png';
	  				break;
	  			case 'comment' :
	  				note.rankUrl = '/content/img/status_message.png';
	  				break;
	  		}

	  		var noteHtml = studentNoteTemplate(note);
	  		studentNotesDiv.append(noteHtml);

	  		studentNotesDiv.find('.studentNote[data-id="' + note.Id + '"]').find('.noteGreenFlag').tooltip({
	  		    title: 'Resolution: ' + note.Resolution
	  		});

	  		studentNotesDiv.find('.studentNote[data-id="' + note.Id + '"]').find('.mailIcon').tooltip({
	  			title: 'Sent to: ' + note.EmailRecipients
	  		});

	  		// switch positions
	  		//position = position === 'Left' ? 'Right' : 'Left';
	  	});
	  }


	  /**
	   *
	   */
	 function removeNoteBtnClickHandler(event) {	     
	     var parent = $(event.currentTarget).parents('.studentNote');
	     $.ajax({
	         type: 'POST',
	         url: '/Api/DeleteNote',
	         data: {
	             studentId: selectedStudentId,
                 noteId: parent.data('id')
	         },
	         success: function () {
	             parent.hide('fade');
	         }
	     });
	  }

	 function noteRedFlagClickHandler(event) {
	     var noteId = $(event.currentTarget).data('note-id');
	     $("#flagNoteId").val(noteId);
	     toggleNewNoteForm();
	     toggleNoteFields('flag');
	 }

	  /**
	   *
	   */
	 function addNoteClickHandler() {
	    toggleNewNoteForm();
	  	toggleNoteFields('discipline');
	  }

	 function closeNewNoteFormClickHandler() {
	     toggleNewNoteForm();
	  }

	 function toggleNewNoteForm() {
	      newNoteFormElement.find('input[type="text"]').val('');
	      mainContentElement.toggle('slide', 600);
	      newNoteFormElement.toggle('slide', { duration: 600, direction: 'right' });	  	
	  }

	  function newNoteRankDropdownChangeHandler(event) {
	  	var selectedRank = newNoteRankDropdown.val();
	  	toggleNoteFields(selectedRank);
	  }

	  /**
	   *
	   */
	  function saveNewNoteClickHandler(event) {

	      if ($(".flagNoteId").css('display') !== 'none') {
	          var noteId = $("#flagNoteId").val();
	          var noteDescription = $("#newNoteDescription").val();
	          $.ajax({
	              type: 'POST',
	              url: '/Api/MarkNoteAsResolved',
	              data: {
	                  studentId: selectedStudentId,
	                  noteId: noteId,
	                  description: noteDescription
	              },
	              success: function (result) {
	                  if (result.success) {
	                      $.ajax({
	                          type: 'GET',
	                          url: '/Api/GetNotes?studentId=' + selectedStudentId,
	                          success: function (result) {
	                              var notes = result.Notes;
	                              allNotes = notes;
	                              displayStudentNotes(notes);

	                              // discipline charts uses the notes
	                              drawDisciplineChart(notes);
	                          }
	                      });
	                      toggleNewNoteForm();
	                  }
	              }
	          });
	      } else {

	          var url = '/Api/AddNote',
              noteType = $("#newNoteRankDropdown").val(),
              newNoteObject = {
                  StudentId: selectedStudentId,
                  NoteType: noteType,
                  Subject: $("#newNoteTitleTxt").val(),
                  Text: $("#newNoteDescription").val()
              };

	          if (noteType === 'discipline') {
	              // url = '/Api/AddDisciplineIncident',
	              newNoteObject.Location = $("#newNoteLocation").val();
	              newNoteObject.IncidentTime = $("#newNoteDateTime").val();
	              newNoteObject.BehaviorType = $("#newNoteBehaviorType").val();
	          }

	          if ($("#newNoteSendToParent").is(":checked")) {
	              newNoteObject.EmailNotificationSent = true;
	              newNoteObject.EmailRecipients = 'mikeng13@gmail.com';
	          }

	          $.ajax({
	              'type': 'POST',
	              'url': url,
	              'data': newNoteObject,
	              'success': saveNewNoteSuccessHandler
	          });
	      }
	  }

	  /**
	   *
	   */
	  function saveNewNoteSuccessHandler(result) {
	      var notes = result.Notes;
	      displayStudentNotes(notes);
	      drawDisciplineChart(notes);	      
	  	  toggleNewNoteForm();
	  }

	  /**
	   *
	   */
	  function cancelNewNoteClickHandler(event) {	  	
	  	newNoteDiv.hide();
	  }
	  /////////////////////////
	  //// UI INTERACTIONS ////
	  /////////////////////////
	  /**
	   *
	   */
	  function adjustContentHeight() {
	  	// grab the height of the student details div
	  	var studentDetailsHeight = studentDetailsContainerDiv.height(),
	  		contentHeight = contentElement.height(),
	  		remainingHeight = contentHeight - studentDetailsHeight,
	  		studentNotesMarginTop = parseInt(studentNotesContainerDiv.css('margin-top'));

	  	studentNotesContainerDiv.height(remainingHeight - studentNotesMarginTop);
	  }

	  /**
	   *
	   */
	  function toggleSideBar() {
	  	sidebarDiv.toggle('slide');
	  }

	  /**
	   *
	   */
	  function toggleStudentDetails() {
	  	studentDetailsDiv.slideToggle();
	  }

	  /**
	   *
	   */
	  function toggleNoteFields(rank) {
	  	newNoteFormElement.find('.newNoteField').hide();
	  	newNoteFormElement.find('.' + rank).show();
	  }
	// initialize the app
	init();
})();
