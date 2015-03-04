$.widget('nci.deletelist', {
	version: "0.1",
	defaultElement: '<div>',
	options: {
		valueSeparator: '|',
		nameSeparator: '_',
		emptyListMessage: "Select \"Add More\"",
		emptyListMessageRoot: 'EmptyMessage',
		displayListRoot: 'DisplayList', // The visible list of items
		valueListRoot: 'ValueList', // Hidden list of values
		nameListRoot: 'NameList', // Hidden list of item names
		deletedListRoot: 'DeletedList', // Hidden list of flags for which item to delete
		deleteText: 'Delete'
	},

	_create: function () {
		var deleteListId = this.element.attr('id');
		this.ids = {
			element: deleteListId,
			emptyListMessage: deleteListId + this.options.nameSeparator + this.options.emptyListMessageRoot,
			displayList: deleteListId + this.options.nameSeparator + this.options.displayListRoot,
			valueList: deleteListId + this.options.nameSeparator + this.options.valueListRoot,
			nameList: deleteListId + this.options.nameSeparator + this.options.nameListRoot,
			deletedList: deleteListId + this.options.nameSeparator + this.options.deletedListRoot
		};

		this.element.addClass('nci-deletelist');

		// initialize empty list message
		this.emptyListMessage = $('<div>')
			.attr('id', this.ids.emptyListMessage)
			.text(this.options.emptyListMessage)
			.appendTo(this.element)
			.hide();

		// initialize <ul>
		this.displayList = $('<ul>')
			.attr('id', this.ids.displayList)
			.appendTo(this.element);

		// initialize <input> list
		this.valueList = $('<input>')
			.attr({
				'type': 'hidden',
				'value': '',
				'id': this.ids.valueList,
				'name': this.ids.valueList
			}).appendTo(this.element);

		this.nameList = $('<input>')
			.attr({
				'type': 'hidden',
				'value': '',
				'id': this.ids.nameList,
				'name': this.ids.nameList
			}).appendTo(this.element);

		this.deletedList = $('<input>')
			.attr({
				'type': 'hidden',
				'value': '',
				'id': this.ids.deletedList,
				'name': this.ids.deletedList
			}).appendTo(this.element);

		this.items = [];
	},

	_renderItem: function (ul, item) {
		var deleteButton = $('<button>')
			.attr('type', 'button')
			.addClass('pseudo-icon-deletelist')
			.append($('<div>')
				.addClass('hidden')
				.text(this.options.deleteText)
			);

		var listItem = $('<li>')
			.text(item.name)
			.prepend(deleteButton)
			.appendTo(ul)
			.on('click', deleteButton, function (e) {
				this.deleteItem($(e.target));
			});

		return listItem;
	},

	addItem: function (item) {
		if (typeof item === 'object' && typeof item.name !== 'undefined' && typeof item.value !== 'undefined') {
			// set the item's visibility to true
			item.visible = true;

			// render the item
			this._renderItem(this.displayList, item)
				.data("nci-deletelist-item", item);

			// add the item to the hidden <input> elements
			var separator = this.options.valueSeparator;
			if (this.nameList.prop('value') === '')
				separator = '';
			this.valueList.prop('value', this.valueList.prop('value') + separator + item.value);
			this.nameList.prop('value', this.nameList.prop('value') + separator + item.name);
			this.deletedList.prop('value', this.deletedList.prop('value') + separator + "false");

			this.items.push(item);
			if (this.displayList.children('li:visible').length === 1) {
				this.emptyListMessage.hide();
			}
		}
	},

	deleteItem: function (item) {
		if (typeof item === 'string' || (typeof item === 'object' && typeof item.name !== 'undefined')) {

			// find the item
			var itemName,
			    itemIndex;
			if (typeof item === 'string') {
				itemName = item;
			} else if (typeof item === 'object' && typeof item.name !== 'undefined') {
				itemName = item.name;
			}
			var thisLi = $(this.displayList).children('li:data("nci-deletelist-item")')
				.filter(function (i, el) {
					return $(el).data('nci-deletelist-item').name === itemName;
				});

			// set the item's "visible" data attribute
			var thisLiData = thisLi.data('nci-deletelist-item');
			thisLiData.visible = false;
			thisLi.data('nci-deletelist-item', thisLiData);

			// set the item's "deleted" flag in the deleteList
			var deletionFlags = this.deletedList.prop('value').split(this.options.valueSeparator);
			deletionFlags[thisLi.index()] = "true";
			this.deletedList.prop('value', deletionFlags.join(this.options.valueSeparator));

			// actually hide the item, and show the empty list message if all are hidden
			thisLi.hide();
			if (this.displayList.children('li:visible').length === 0) {
				this.emptyListMessage.show();
			}
		}
	},

	clearAll: function () {
		for (var i = 0; i < this.items.length; i++) {
			this.deleteItem(this.items[i]);
		}
	},

	getItems: function () {
		return this.items;
	},

	_destroy: function () {
		this.element.removeClass('nci-deletelist');
		$.each([
			this.emptyListMessage,
			this.displayList,
			this.valueList,
			this.nameList,
			this.deletedList
		], function (i, thing) {
			thing.remove();
		});
	}

});
